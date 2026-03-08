using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services
{
    public class DkbCsvParser : IStatementParser
    {
        public IEnumerable<Transaction> Parse(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream, Encoding.UTF8, true);
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            // Find header line (starts with Buchungsdatum)
            var headerIndex = lines.FindIndex(l => l != null && l.Contains("Buchungsdatum"));
            if (headerIndex < 0) yield break;

            for (int i = headerIndex + 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                // split by ; but fields are wrapped in quotes
                var parts = SplitCsvLine(line);
                if (parts.Length < 9) continue;

                var buchungstag = parts[0].Trim('"');
                var wertstellung = parts[1].Trim('"');
                var status = parts[2].Trim('"');
                var zahlungspflichtige = parts[3].Trim('"');
                var zahlungsempfaenger = parts[4].Trim('"');
                var verwendungszweck = parts[5].Trim('"');
                var umsatztyp = parts[6].Trim('"');
                var iban = parts[7].Trim('"');
                var betragRaw = parts[8].Trim('"');

                if (!TryParseDecimal(betragRaw, out var betrag)) continue;

                var t = new Transaction
                {
                    Datum = ParseGermanDate(buchungstag),
                    Betrag = betrag,
                    Titel = zahlungsempfaenger ?? verwendungszweck ?? string.Empty,
                    KategorieId = string.Empty
                };

                yield return t;
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            var parts = new List<string>();
            var cur = new System.Text.StringBuilder();
            bool inQuotes = false;
            foreach (var ch in line)
            {
                if (ch == '"') { inQuotes = !inQuotes; cur.Append(ch); continue; }
                if (ch == ';' && !inQuotes) { parts.Add(cur.ToString()); cur.Clear(); continue; }
                cur.Append(ch);
            }
            parts.Add(cur.ToString());
            return parts.ToArray();
        }

        private static DateTime ParseGermanDate(string s)
        {
            if (DateTime.TryParseExact(s.Trim('"'), "dd.MM.yy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var d)) return d;
            if (DateTime.TryParse(s.Trim('"'), CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out d)) return d;
            return DateTime.Today;
        }

        private static bool TryParseDecimal(string s, out decimal value)
        {
            value = 0;
            s = s.Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
            s = s.Replace("€", string.Empty).Trim();
            s = s.Replace('.', ',');
            return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.GetCultureInfo("de-DE"), out value);
        }
    }
}
