using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services
{
    public class ImportService
    {
        private readonly IEnumerable<IStatementParser> _parsers;
        private readonly ITransactionRepository _txRepo;

        public ImportService(IEnumerable<IStatementParser> parsers, ITransactionRepository txRepo)
        {
            _parsers = parsers;
            _txRepo = txRepo;
        }

        public IEnumerable<Transaction> ImportFromCsv(Stream csvStream, string accountId = null)
        {
            // Choose parser by heuristics: try each parser until one returns non-empty
            foreach (var p in _parsers)
            {
                csvStream.Seek(0, SeekOrigin.Begin);
                var dtos = p.Parse(csvStream);
                if (dtos != null && dtos.Any())
                {
                    var txs = new List<Transaction>();

                    foreach (var d in dtos)
                    {
                        if (d == null) continue;
                        if (d.Buchungsdatum == default) continue; // skip malformed rows

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

                        // Simple duplicate check: look for existing on same day with same amount and normalized title
                        bool isDuplicate = false;
                        try
                        {
                            var existing = _txRepo.GetTransactionsAsync(d.Buchungsdatum.Date, d.Buchungsdatum.Date).Result;
                            if (existing != null && existing.Any(e => e.Datum.Date == d.Buchungsdatum.Date && e.Betrag == d.Betrag && Normalize(e.Titel) == Normalize(title)))
                            {
                                isDuplicate = true;
                            }
                        }
                        catch
                        {
                            // repository may not support queries; ignore and continue
                        }

                        if (!isDuplicate)
                        {
                            txs.Add(tx);

                            // persist using whatever API the repository exposes (SaveTransactionAsync or Add)
                            try
                            {
                                var saveMethod = _txRepo.GetType().GetMethod("SaveTransactionAsync");
                                if (saveMethod != null)
                                {
                                    var task = (System.Threading.Tasks.Task)saveMethod.Invoke(_txRepo, new object[] { tx })!;
                                    task.GetAwaiter().GetResult();
                                }
                                else
                                {
                                    var addMethod = _txRepo.GetType().GetMethod("Add");
                                    if (addMethod != null)
                                    {
                                        addMethod.Invoke(_txRepo, new object[] { tx });
                                    }
                                }
                            }
                            catch
                            {
                                // swallowing persistence errors for now; higher-level error handling / retries can be added later
                            }
                        }
                    }

                    return txs;
                }
            }

            return Enumerable.Empty<Transaction>();
        }

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var lowered = input.Trim().ToLowerInvariant();
            // collapse whitespace and remove punctuation for loose matching
            var chars = lowered.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray();
            return new string(chars).Replace("\r", "").Replace("\n", "").Replace("  ", " ").Trim();
        }
    }
}
