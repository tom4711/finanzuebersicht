using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Services
{
    public class ImportService(
        IEnumerable<IStatementParser> parsers,
        ITransactionRepository transactionRepository,
        ILogger<ImportService> logger,
        ICategoryRepository? categoryRepository = null,
        CategorizationService? categorizationService = null)
    {
        private readonly IEnumerable<IStatementParser> _parsers = parsers;
        private readonly ITransactionRepository _transactionRepository = transactionRepository;
        private readonly ILogger<ImportService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICategoryRepository? _categoryRepository = categoryRepository;
        private readonly CategorizationService? _categorizationService = categorizationService;

        public async System.Threading.Tasks.Task<ImportResult> ImportFromCsvAsync(
            System.IO.Stream csvStream,
            string? accountId = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("ImportService: starting import (accountId={AccountId})", accountId ?? "(none)");

            // 1. Read stream into memory
            byte[] buffer;
            try
            {
                using var ms = new MemoryStream();
                await csvStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                buffer = ms.ToArray();
                _logger.LogDebug("ImportService: read input stream {Bytes} bytes", buffer.Length);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportService: failed to read input stream");
                return new ImportResult { ErrorMessage = $"Fehler beim Lesen der Datei: {ex.Message}" };
            }

            // 2. Find a parser that returns records
            var parserList = _parsers?.ToList() ?? [];
            _logger.LogInformation("ImportService: available parsers={Count}", parserList.Count);

            List<TransactionDto>? dtosList = null;
            foreach (var parser in parserList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var parserStream = new MemoryStream(buffer, writable: false);
                try
                {
                    dtosList = parser.Parse(parserStream)?.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ImportService: parser {Parser} threw during Parse", parser.GetType().Name);
                    dtosList = null;
                    continue;
                }

                if (dtosList is { Count: > 0 })
                {
                    _logger.LogInformation("ImportService: parser {Parser} matched with {Count} records",
                        parser.GetType().Name, dtosList.Count);
                    break;
                }

                dtosList = null;
            }

            if (dtosList is null or { Count: 0 })
            {
                _logger.LogInformation("ImportService: no parser matched or no records found");
                return new ImportResult { ErrorMessage = "Kein passender Parser gefunden oder keine Datensätze in der Datei." };
            }

            // 3. Validate and build transaction list (no saves yet)
            var toImport = new List<Transaction>();
            var duplicates = new List<Transaction>();
            var skippedMalformed = 0;

            foreach (var dto in dtosList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (dto is null || dto.Buchungsdatum == default)
                {
                    skippedMalformed++;
                    _logger.LogDebug("ImportService: skipping malformed DTO");
                    continue;
                }

                var title = BuildTitle(dto);
                var transaction = new Transaction
                {
                    Betrag = dto.Betrag,
                    Datum = dto.Buchungsdatum,
                    Titel = title,
                    Verwendungszweck = dto.Verwendungszweck ?? string.Empty,
                    KategorieId = string.Empty,
                    Typ = dto.Betrag >= 0 ? TransactionType.Einnahme : TransactionType.Ausgabe,
                    AccountId = accountId ?? dto.SourceAccountId
                };

                // Duplicate check
                bool isDuplicate = false;
                try
                {
                    var from = dto.Buchungsdatum.Date.AddDays(-1);
                    var to = dto.Buchungsdatum.Date.AddDays(1);
                    var existing = await _transactionRepository.GetTransactionsAsync(from, to).ConfigureAwait(false);
                    isDuplicate = existing?.Any(e =>
                        e.Datum.Date >= from && e.Datum.Date <= to &&
                        e.Betrag == dto.Betrag &&
                        Normalize(e.Titel) == Normalize(title)) ?? false;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ImportService: duplicate check failed for '{Title}'", title);
                    // On duplicate check failure, treat as non-duplicate (safer: import rather than lose data)
                }

                if (isDuplicate)
                {
                    _logger.LogDebug("ImportService: duplicate detected for '{Title}' {Amount} on {Date}",
                        title, dto.Betrag, dto.Buchungsdatum);
                    duplicates.Add(transaction);
                    continue;
                }

                // Auto-categorize
                transaction.KategorieId = await ResolveCategoryAsync(dto, transaction, cancellationToken)
                    .ConfigureAwait(false);

                toImport.Add(transaction);
            }

            // 4. Save all valid transactions (bulk — fail individually but continue for others)
            var saveErrors = new List<string>();
            var imported = new List<Transaction>();

            foreach (var transaction in toImport)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _transactionRepository.SaveTransactionAsync(transaction).ConfigureAwait(false);
                    imported.Add(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ImportService: failed to save transaction '{Title}' {Amount} on {Date}",
                        transaction.Titel, transaction.Betrag, transaction.Datum);
                    saveErrors.Add($"{transaction.Titel} ({transaction.Datum:dd.MM.yyyy}): {ex.Message}");
                }
            }

            _logger.LogInformation(
                "ImportService: finished — imported={I}, duplicates={D}, malformed={M}, saveErrors={E}",
                imported.Count, duplicates.Count, skippedMalformed, saveErrors.Count);

            return new ImportResult
            {
                Imported = imported,
                Duplicates = duplicates,
                SkippedMalformed = skippedMalformed,
                SaveErrors = saveErrors,
            };
        }

        private static string BuildTitle(TransactionDto dto)
        {
            var title = !string.IsNullOrWhiteSpace(dto.Zahlungsempfaenger)
                ? dto.Zahlungsempfaenger
                : !string.IsNullOrWhiteSpace(dto.Zahlungspflichtige)
                    ? dto.Zahlungspflichtige
                    : dto.Verwendungszweck;

            return string.IsNullOrWhiteSpace(title)
                ? (string.IsNullOrWhiteSpace(dto.Verwendungszweck)
                    ? $"Buchung {dto.Buchungsdatum:dd.MM.yyyy} {dto.Betrag:0.00}€"
                    : dto.Verwendungszweck)
                : title;
        }

        private async System.Threading.Tasks.Task<string> ResolveCategoryAsync(
            TransactionDto dto,
            Transaction transaction,
            CancellationToken cancellationToken)
        {
            if (_categoryRepository is null)
                return string.Empty;

            List<Category>? categories = null;
            try
            {
                categories = await _categoryRepository.GetCategoriesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to load categories for auto-categorization; attempting Unkategorisiert fallback");
                // categories stays null — fall through to Unkategorisiert fallback below
            }

            if (categories is { Count: > 0 } && _categorizationService is not null)
            {
                try
                {
                    var category = await _categorizationService
                        .CategorizAsync(dto, categories, cancellationToken).ConfigureAwait(false);
                    if (category is not null)
                    {
                        _logger.LogDebug("ImportService: auto-categorized '{Title}' → {Category}",
                            transaction.Titel, category.Name);
                        return category.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ImportService: categorization failed for '{Title}'", transaction.Titel);
                }
            }

            // Fallback to "Unkategorisiert" system category
            try
            {
                var unclassified = categories?.FirstOrDefault(c =>
                    c.SystemKey == Finanzuebersicht.Constants.SystemCategoryKeys.Unkategorisiert ||
                    c.Name == "Unkategorisiert");

                if (unclassified is null)
                {
                    unclassified = new Category
                    {
                        Name = "Unkategorisiert",
                        Icon = "❓",
                        Color = "#A2845E",
                        Typ = TransactionType.Ausgabe,
                        SystemKey = Finanzuebersicht.Constants.SystemCategoryKeys.Unkategorisiert
                    };
                    await _categoryRepository.SaveCategoryAsync(unclassified).ConfigureAwait(false);
                }

                return unclassified.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to ensure default category");
                return string.Empty;
            }
        }

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var lowered = input.Trim().ToLowerInvariant();
            var normalized = lowered.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var cleaned = new string([.. sb.ToString().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))]);
            return Regex.Replace(cleaned, "\\s+", " ").Trim();
        }
    }
}
