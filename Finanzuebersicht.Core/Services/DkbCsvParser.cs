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
        public IEnumerable<TransactionDto> Parse(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream, Encoding.UTF8, true);
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                var l = reader.ReadLine();
                if (l != null) lines.Add(l);
            }

            // Find header line (starts with Buchungsdatum)
            var headerIndex = lines.FindIndex(l => l.Contains("Buchungsdatum"));
            if (headerIndex < 0) yield break;

            for (int i = headerIndex + 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = SplitCsvLine(line);
                if (parts.Length < 9) continue;

                var dto = new TransactionDto();
                dto.Buchungsdatum = ParseGermanDate(parts[0]);
                dto.Wertstellung = ParseGermanDate(parts[1]);
                dto.Status = parts[2].Trim('"');
                dto.Zahlungspflichtige = parts[3].Trim('"');
                dto.Zahlungsempfaenger = parts[4].Trim('"');
                dto.Verwendungszweck = parts[5].Trim('"');
                dto.Umsatztyp = parts[6].Trim('"');
                dto.IBAN = parts[7].Trim('"');
                TryParseDecimal(parts[8].Trim('"'), out var betrag);
                dto.Betrag = betrag;
                dto.GlueubigerId = parts.Length > 9 ? parts[9].Trim('"') : string.Empty;
                dto.Mandatsreferenz = parts.Length > 10 ? parts[10].Trim('"') : string.Empty;
                dto.Kundenreferenz = parts.Length > 11 ? parts[11].Trim('"') : string.Empty;

                yield return dto;
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
            if (string.IsNullOrWhiteSpace(s)) return false;
            // remove non-breaking spaces and normal spaces
            s = s.Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
            // remove euro sign
            s = s.Replace("€", string.Empty).Trim();
            // remove thousand separator '.' used in German formatting, keep comma as decimal separator
            s = s.Replace(".", string.Empty);
            return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.GetCultureInfo("de-DE"), out value);
        }
    }
}
