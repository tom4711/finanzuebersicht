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

namespace Finanzuebersicht.Core.Services
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

        public async System.Threading.Tasks.Task<IEnumerable<Transaction>> ImportFromCsvAsync(System.IO.Stream csvStream, string? accountId = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("ImportService: starting import (accountId={AccountId})", accountId ?? "(none)");
                try { FileLogger.Append("ImportService", $"starting import (accountId={accountId ?? "(none)"})"); } catch { }

                if (_transactionRepository == null)
                {
                    _logger?.LogError("ImportService: ITransactionRepository is null - cannot persist transactions");
                    return [];
                }

                // read incoming stream fully into memory to avoid platform-specific SecurityScopedStream issues
                byte[] buffer;
                try
                {
                    using var ms = new MemoryStream();
                    await csvStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                    buffer = ms.ToArray();
                    try { FileLogger.Append("ImportService", $"read input stream {buffer.Length} bytes"); } catch { }
                }
                catch (System.Exception ex)
                {
                    _logger?.LogError(ex, "ImportService: failed to read input stream");
                    try { FileLogger.Append("ImportService", "failed to read input stream", ex); } catch { }
                    return [];
                }

                var parserList = _parsers?.ToList() ?? [];
                _logger?.LogInformation("ImportService: available parsers={Count}", parserList.Count);

                // Choose parser by heuristics: try each parser until one returns non-empty
                foreach (var p in parserList)
                {
                    // create a fresh MemoryStream for each parser so parser disposal won't affect other attempts
                    using var parserStream = new MemoryStream(buffer, writable: false);
                    List<TransactionDto>? dtosList = null;
                    try
                    {
                        // materialize results within the using block so parser can dispose the reader/stream safely
                        dtosList = p.Parse(parserStream)?.ToList();
                    }
                    catch (System.Exception ex)
                    {
                        _logger?.LogWarning(ex, "ImportService: parser {Parser} threw during Parse", p.GetType().FullName);
                        try { FileLogger.Append("ImportService", $"parser {p.GetType().FullName} threw during Parse", ex); } catch { }
                        continue;
                    }

                    var count = dtosList?.Count ?? 0;
                    _logger?.LogInformation("ImportService: parser {Parser} returned {Count} records", p.GetType().FullName, count);
                    try { FileLogger.Append("ImportService", $"parser {p.GetType().FullName} returned {count} records"); } catch { }

                    if (dtosList != null && dtosList.Count != 0)
                    {
                        var importedTransactions = new List<Transaction>();

                        foreach (var importedRecord in dtosList)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (importedRecord == null)
                            {
                                _logger?.LogWarning("ImportService: skipping null DTO");
                                continue;
                            }

                            if (importedRecord.Buchungsdatum == default)
                            {
                                _logger?.LogWarning("ImportService: skipping malformed DTO with empty Buchungsdatum: {Dto}", importedRecord);
                                continue; // skip malformed rows
                            }

                            var title = !string.IsNullOrWhiteSpace(importedRecord.Zahlungsempfaenger)
                                ? importedRecord.Zahlungsempfaenger
                                : !string.IsNullOrWhiteSpace(importedRecord.Zahlungspflichtige)
                                    ? importedRecord.Zahlungspflichtige
                                    : importedRecord.Verwendungszweck;

                            var finalTitle = string.IsNullOrWhiteSpace(title) ? (string.IsNullOrWhiteSpace(importedRecord.Verwendungszweck) ? $"Buchung {importedRecord.Buchungsdatum:dd.MM.yyyy} {importedRecord.Betrag:0.00}€" : importedRecord.Verwendungszweck) : title;
                            var transaction = new Transaction
                            {
                                Betrag = importedRecord.Betrag,
                                Datum = importedRecord.Buchungsdatum,
                                Titel = finalTitle,
                                Verwendungszweck = importedRecord.Verwendungszweck ?? string.Empty,
                                KategorieId = string.Empty,
                                Typ = importedRecord.Betrag >= 0 ? TransactionType.Einnahme : TransactionType.Ausgabe,
                                AccountId = accountId ?? importedRecord.SourceAccountId
                            };

                            // Improved duplicate check: look for nearby dates (+/-1 day) with same amount and normalized title
                            bool isDuplicate = false;
                            try
                            {
                                var from = importedRecord.Buchungsdatum.Date.AddDays(-1);
                                var to = importedRecord.Buchungsdatum.Date.AddDays(1);
                                var existing = await _transactionRepository.GetTransactionsAsync(from, to).ConfigureAwait(false);
                                if (existing != null && existing.Any(e => e.Datum.Date >= from && e.Datum.Date <= to && e.Betrag == importedRecord.Betrag && Normalize(e.Titel) == Normalize(title)))
                                {
                                    isDuplicate = true;
                                    _logger?.LogInformation("ImportService: duplicate detected for '{Title}' amount {Amount} on {Date}", title, importedRecord.Betrag, importedRecord.Buchungsdatum);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                _logger?.LogWarning(ex, "ImportService: duplicate check failed");
                                 try { FileLogger.Append("ImportService", "duplicate check failed", ex); } catch { }
                            }

                            if (!isDuplicate)
                            {
                                // Auto-categorize using CategorizationService if available
                                if (_categorizationService != null && _categoryRepository != null && string.IsNullOrWhiteSpace(transaction.KategorieId))
                                {
                                    try
                                    {
                                        var categories = await _categoryRepository.GetCategoriesAsync().ConfigureAwait(false);
                                        if (categories != null && categories.Count > 0)
                                        {
                                            var category = await _categorizationService.CategorizAsync(importedRecord, categories, cancellationToken).ConfigureAwait(false);
                                            if (category != null)
                                            {
                                                transaction.KategorieId = category.Id;
                                                _logger?.LogInformation("ImportService: auto-categorized '{Title}' → {Category}", transaction.Titel, category.Name);
                                            }
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        _logger?.LogWarning(ex, "ImportService: categorization failed, falling back to Unkategorisiert");
                                        try { FileLogger.Append("ImportService", "categorization failed, using fallback", ex); } catch { }
                                    }
                                }

                                // Fallback: ensure an "Unkategorisiert" category exists if still no category assigned
                                if (_categoryRepository != null && string.IsNullOrWhiteSpace(transaction.KategorieId))
                                {
                                    try
                                    {
                                        var categories = await _categoryRepository.GetCategoriesAsync().ConfigureAwait(false);
                                        var unclassifiedCategory = categories?.FirstOrDefault(c => c.SystemKey == Finanzuebersicht.Core.Constants.SystemCategoryKeys.Unkategorisiert || c.Name == "Unkategorisiert");
                                        if (unclassifiedCategory == null)
                                        {
                                            unclassifiedCategory = new Finanzuebersicht.Models.Category
                                            {
                                                Name = "Unkategorisiert",
                                                Icon = "❓",
                                                Color = "#A2845E",
                                                Typ = Finanzuebersicht.Models.TransactionType.Ausgabe,
                                                SystemKey = Finanzuebersicht.Core.Constants.SystemCategoryKeys.Unkategorisiert
                                            };
                                            await _categoryRepository.SaveCategoryAsync(unclassifiedCategory).ConfigureAwait(false);
                                            try { FileLogger.Append("ImportService", "created default 'Unkategorisiert' category"); } catch { }
                                        }

                                        if (unclassifiedCategory != null)
                                            transaction.KategorieId = unclassifiedCategory.Id;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        _logger?.LogWarning(ex, "ImportService: failed to ensure default category");
                                    }
                                }

                                importedTransactions.Add(transaction);

                                // persist using repository API (SaveTransactionAsync)
                                try
                                {
                                    await _transactionRepository.SaveTransactionAsync(transaction).ConfigureAwait(false);
                                    _logger?.LogInformation("ImportService: saved transaction '{Title}' amount {Amount} on {Date}", transaction.Titel, transaction.Betrag, transaction.Datum);
                                }
                                catch (System.Exception ex)
                                {
                                    _logger?.LogError(ex, "ImportService: failed to save transaction {Title} amount {Amount} on {Date}", transaction.Titel, transaction.Betrag, transaction.Datum);
                                         try { FileLogger.Append("ImportService", $"failed to save transaction {transaction.Titel} amount {transaction.Betrag} on {transaction.Datum}", ex); } catch { }
                                }
                            }
                        }

                        _logger?.LogInformation("ImportService: finished importing {Count} transactions", importedTransactions.Count);
                        return importedTransactions;
                    }
                }

                _logger?.LogInformation("ImportService: no parser matched or no records found");
                return [];
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "ImportService: unexpected error during import");
                try { FileLogger.Append("ImportService", "unexpected error during import", ex); } catch { }
                throw;
            }
        }

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var lowered = input.Trim().ToLowerInvariant();
            // remove diacritics
            var normalized = lowered.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var cleaned = new string([.. sb.ToString().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))]);
            // collapse whitespace
            return Regex.Replace(cleaned, "\\s+", " ").Trim();
        }
    }
}
