using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Finanzuebersicht.Core.Services
{
    public class DkbCsvParser : IStatementParser
    {
        public IEnumerable<TransactionDto> Parse(Stream csvStream)
        {
            // Be defensive: read full content into a string so parser isn't sensitive to stream capabilities
            if (csvStream == null) yield break;
            string content;
            try
            {
                try { if (csvStream.CanSeek) csvStream.Position = 0; } catch { }
                using var ms = new MemoryStream();
                csvStream.CopyTo(ms);
                content = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DkbCsvParser: failed to read stream: {ex.Message}");
                yield break;
            }

            var records = ParseCsv(content, ';');

            // find header row where first cell equals Buchungsdatum
            var headerIndex = records.FindIndex(r => r.Length > 0 && r[0].Trim('"', ' ') == "Buchungsdatum");
            if (headerIndex < 0) yield break;

            for (int i = headerIndex + 1; i < records.Count; i++)
            {
                var parts = records[i];
                if (parts.Length == 0) continue;

                TransactionDto? dto = null;
                try
                {
                    // pad to expected length
                    var p = parts.Concat(Enumerable.Repeat(string.Empty, Math.Max(0, 12 - parts.Length))).ToArray();

                    // require Buchungsdatum to be a valid date in expected format; otherwise treat row as malformed
                    if (!TryParseGermanDateExact(p[0], out var buchung))
                    {
                        throw new System.FormatException($"Invalid Buchungsdatum: {p[0]}");
                    }

                    dto = new TransactionDto();
                    dto.Buchungsdatum = buchung;
                    dto.Wertstellung = ParseGermanDate(p[1]);
                    dto.Status = p[2].Trim('"');
                    dto.Zahlungspflichtige = p[3].Trim('"');
                    dto.Zahlungsempfaenger = p[4].Trim('"');
                    dto.Verwendungszweck = p[5].Trim('"');
                    dto.Umsatztyp = p[6].Trim('"');
                    dto.IBAN = p[7].Trim('"');
                    TryParseDecimal(p[8].Trim('"'), out var betrag);
                    dto.Betrag = betrag;
                    dto.GlueubigerId = p[9].Trim('"');
                    dto.Mandatsreferenz = p[10].Trim('"');
                    dto.Kundenreferenz = p[11].Trim('"');
                }
                catch (System.Exception ex)
                {
                    // skip malformed/invalid rows, but keep running
                    System.Diagnostics.Debug.WriteLine($"DkbCsvParser: skipping row {i} due to parse error: {ex.Message}");
                }

                if (dto != null) yield return dto;
            }
        }

        private static List<string[]> ParseCsv(string content, char sep)
        {
            var records = new List<string[]>();
            var fields = new List<string>();
            var cur = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < content.Length; i++)
            {
                var ch = content[i];
                if (ch == '"')
                {
                    // handle escaped double quote ""
                    if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                    {
                        cur.Append('"');
                        i++;
                        continue;
                    }
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && ch == sep)
                {
                    fields.Add(cur.ToString());
                    cur.Clear();
                    continue;
                }

                if (!inQuotes && (ch == '\r' || ch == '\n'))
                {
                    // handle CRLF
                    if (ch == '\r' && i + 1 < content.Length && content[i + 1] == '\n') i++;
                    fields.Add(cur.ToString());
                    cur.Clear();
                    records.Add(fields.ToArray());
                    fields.Clear();
                    continue;
                }

                cur.Append(ch);
            }

            // flush remaining
            if (cur.Length > 0 || fields.Count > 0)
            {
                fields.Add(cur.ToString());
                records.Add(fields.ToArray());
            }

            return records;
        }

        private static DateTime ParseGermanDate(string s)
        {
            var str = s?.Trim('"', ' ');
            if (string.IsNullOrWhiteSpace(str)) return DateTime.Today;
            if (DateTime.TryParseExact(str, "dd.MM.yy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var d)) return d;
            if (DateTime.TryParse(str, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out d)) return d;
            return DateTime.Today;
        }

        private static bool TryParseGermanDateExact(string s, out DateTime d)
        {
            d = default;
            var str = s?.Trim('"', ' ');
            if (string.IsNullOrWhiteSpace(str)) return false;
            return DateTime.TryParseExact(str, "dd.MM.yy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out d);
        }

        private static bool TryParseDecimal(string s, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
            s = s.Replace("€", string.Empty).Trim();
            s = s.Replace(".", string.Empty);
            return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.GetCultureInfo("de-DE"), out value);
        }
    }
}
