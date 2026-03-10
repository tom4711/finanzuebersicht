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

        public ImportService(IEnumerable<IStatementParser> parsers, ITransactionRepository txRepo, ILogger<ImportService> logger)
        {
            _parsers = parsers;
            _txRepo = txRepo;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async System.Threading.Tasks.Task<IEnumerable<Transaction>> ImportFromCsvAsync(System.IO.Stream csvStream, string accountId = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("ImportService: starting import (accountId={AccountId})", accountId ?? "(none)");

                if (_txRepo == null)
                {
                    _logger?.LogError("ImportService: ITransactionRepository is null - cannot persist transactions");
                    return Enumerable.Empty<Transaction>();
                }

                var parserList = _parsers?.ToList() ?? new List<IStatementParser>();
                _logger?.LogInformation("ImportService: available parsers={Count}", parserList.Count);

                // Choose parser by heuristics: try each parser until one returns non-empty
                foreach (var p in parserList)
                {
                    csvStream.Seek(0, SeekOrigin.Begin);
                    IEnumerable<TransactionDto>? dtos = null;
                    try
                    {
                        dtos = p.Parse(csvStream);
                    }
                    catch (System.Exception ex)
                    {
                        _logger?.LogWarning(ex, "ImportService: parser {Parser} threw during Parse", p.GetType().FullName);
                        continue;
                    }

                    var count = dtos?.Count() ?? 0;
                    _logger?.LogInformation("ImportService: parser {Parser} returned {Count} records", p.GetType().FullName, count);

                    if (dtos != null && dtos.Any())
                    {
                        var txs = new List<Transaction>();

                        foreach (var d in dtos)
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

                            var tx = new Transaction
                            {
                                Betrag = d.Betrag,
                                Datum = d.Buchungsdatum,
                                Titel = title,
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
                            }

                            if (!isDuplicate)
                            {
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
                throw;
            }
        }

        public IEnumerable<Transaction> ImportFromCsv(Stream csvStream, string accountId = null)
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
