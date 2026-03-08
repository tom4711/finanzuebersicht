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
                    // map DTOs to Transaction and persist
                    var txs = dtos.Select(d => new Transaction
                    {
                        Betrag = d.Betrag,
                        Datum = d.Buchungsdatum,
                        Titel = string.IsNullOrWhiteSpace(d.Zahlungsempfaenger) ? d.Verwendungszweck : d.Zahlungsempfaenger,
                        KategorieId = string.Empty,
                    }).ToList();

                    foreach (var tx in txs)
                    {
                        _txRepo.Add(tx);
                    }

                    return txs;
                }
            }

            return Enumerable.Empty<Transaction>();
        }
    }
}
