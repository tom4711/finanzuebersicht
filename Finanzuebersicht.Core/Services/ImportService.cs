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
    public class ImportService
    {
        private readonly IEnumerable<IStatementParser> _parsers;
        private readonly ITransactionRepository _txRepo;
        private readonly ILogger<ImportService> _logger;
        private readonly ICategoryRepository? _categoryRepo;
        private readonly CategorizationService? _categorizationService;

        public ImportService(
            IEnumerable<IStatementParser> parsers,
            ITransactionRepository txRepo,
            ILogger<ImportService> logger,
            ICategoryRepository? categoryRepo = null,
            CategorizationService? categorizationService = null)
        {
            _parsers = parsers;
            _txRepo = txRepo;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _categoryRepo = categoryRepo;
            _categorizationService = categorizationService;
        }

        public async System.Threading.Tasks.Task<IEnumerable<Transaction>> ImportFromCsvAsync(System.IO.Stream csvStream, string? accountId = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("ImportService: starting import (accountId={AccountId})", accountId ?? "(none)");
                try { FileLogger.Append("ImportService", $"starting import (accountId={accountId ?? "(none)"})"); } catch { }

                if (_txRepo == null)
                {
                    _logger?.LogError("ImportService: ITransactionRepository is null - cannot persist transactions");
                    return Enumerable.Empty<Transaction>();
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
                    return Enumerable.Empty<Transaction>();
                }

                var parserList = _parsers?.ToList() ?? new List<IStatementParser>();
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

                    var count = dtosList?.Count() ?? 0;
                    _logger?.LogInformation("ImportService: parser {Parser} returned {Count} records", p.GetType().FullName, count);
                    try { FileLogger.Append("ImportService", $"parser {p.GetType().FullName} returned {count} records"); } catch { }

                    if (dtosList != null && dtosList.Any())
                    {
                        var txs = new List<Transaction>();

                        foreach (var d in dtosList)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (d == null)
                            {
                                _logger?.LogWarning("ImportService: skipping null DTO");
                                continue;
                            }

                            if (d.Buchungsdatum == default)
                            {
                                _logger?.LogWarning("ImportService: skipping malformed DTO with empty Buchungsdatum: {Dto}", d);
                                continue; // skip malformed rows
                            }

                            var title = !string.IsNullOrWhiteSpace(d.Zahlungsempfaenger)
                                ? d.Zahlungsempfaenger
                                : !string.IsNullOrWhiteSpace(d.Zahlungspflichtige)
                                    ? d.Zahlungspflichtige
                                    : d.Verwendungszweck;

                            var finalTitle = string.IsNullOrWhiteSpace(title) ? (string.IsNullOrWhiteSpace(d.Verwendungszweck) ? $"Buchung {d.Buchungsdatum:dd.MM.yyyy} {d.Betrag:0.00}€" : d.Verwendungszweck) : title;
                            var tx = new Transaction
                            {
                                Betrag = d.Betrag,
                                Datum = d.Buchungsdatum,
                                Titel = finalTitle,
                                Verwendungszweck = d.Verwendungszweck ?? string.Empty,
                                KategorieId = string.Empty,
                                Typ = d.Betrag >= 0 ? TransactionType.Einnahme : TransactionType.Ausgabe,
                                AccountId = accountId ?? d.SourceAccountId
                            };

                            // Improved duplicate check: look for nearby dates (+/-1 day) with same amount and normalized title
                            bool isDuplicate = false;
                            try
                            {
                                var from = d.Buchungsdatum.Date.AddDays(-1);
                                var to = d.Buchungsdatum.Date.AddDays(1);
                                var existing = await _txRepo.GetTransactionsAsync(from, to).ConfigureAwait(false);
                                if (existing != null && existing.Any(e => e.Datum.Date >= from && e.Datum.Date <= to && e.Betrag == d.Betrag && Normalize(e.Titel) == Normalize(title)))
                                {
                                    isDuplicate = true;
                                    _logger?.LogInformation("ImportService: duplicate detected for '{Title}' amount {Amount} on {Date}", title, d.Betrag, d.Buchungsdatum);
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
                                if (_categorizationService != null && _categoryRepo != null && string.IsNullOrWhiteSpace(tx.KategorieId))
                                {
                                    try
                                    {
                                        var categories = await _categoryRepo.GetCategoriesAsync().ConfigureAwait(false);
                                        if (categories != null && categories.Count > 0)
                                        {
                                            var category = await _categorizationService.CategorizAsync(d, categories, cancellationToken).ConfigureAwait(false);
                                            if (category != null)
                                            {
                                                tx.KategorieId = category.Id;
                                                _logger?.LogInformation("ImportService: auto-categorized '{Title}' → {Category}", tx.Titel, category.Name);
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
                                if (_categoryRepo != null && string.IsNullOrWhiteSpace(tx.KategorieId))
                                {
                                    try
                                    {
                                        var categories = await _categoryRepo.GetCategoriesAsync().ConfigureAwait(false);
                                        var sys = categories?.FirstOrDefault(c => c.SystemKey == "SysCat_Unkategorisiert" || c.Name == "Unkategorisiert");
                                        if (sys == null)
                                        {
                                            sys = new Finanzuebersicht.Models.Category
                                            {
                                                Name = "Unkategorisiert",
                                                Icon = "❓",
                                                Color = "#A2845E",
                                                Typ = Finanzuebersicht.Models.TransactionType.Ausgabe,
                                                SystemKey = "SysCat_Unkategorisiert"
                                            };
                                            await _categoryRepo.SaveCategoryAsync(sys).ConfigureAwait(false);
                                            try { FileLogger.Append("ImportService", "created default 'Unkategorisiert' category"); } catch { }
                                        }

                                        if (sys != null)
                                            tx.KategorieId = sys.Id;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        _logger?.LogWarning(ex, "ImportService: failed to ensure default category");
                                    }
                                }

                                txs.Add(tx);

                                // persist using repository API (SaveTransactionAsync)
                                try
                                {
                                    await _txRepo.SaveTransactionAsync(tx).ConfigureAwait(false);
                                    _logger?.LogInformation("ImportService: saved transaction '{Title}' amount {Amount} on {Date}", tx.Titel, tx.Betrag, tx.Datum);
                                }
                                catch (System.Exception ex)
                                {
                                    _logger?.LogError(ex, "ImportService: failed to save transaction {Title} amount {Amount} on {Date}", tx.Titel, tx.Betrag, tx.Datum);
                                         try { FileLogger.Append("ImportService", $"failed to save transaction {tx.Titel} amount {tx.Betrag} on {tx.Datum}", ex); } catch { }
                                }
                            }
                        }

                        _logger?.LogInformation("ImportService: finished importing {Count} transactions", txs.Count);
                        return txs;
                    }
                }

                _logger?.LogInformation("ImportService: no parser matched or no records found");
                return Enumerable.Empty<Transaction>();
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "ImportService: unexpected error during import");
                try { FileLogger.Append("ImportService", "unexpected error during import", ex); } catch { }
                throw;
            }
        }

        [Obsolete("Use ImportFromCsvAsync instead. Sync-over-async pattern can cause deadlocks in UI contexts.")]
        public IEnumerable<Transaction> ImportFromCsv(Stream csvStream, string? accountId = null)
        {
            return ImportFromCsvAsync(csvStream, accountId).GetAwaiter().GetResult();
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
            var cleaned = new string(sb.ToString().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            // collapse whitespace
            return Regex.Replace(cleaned, "\\s+", " ").Trim();
        }
    }
}
