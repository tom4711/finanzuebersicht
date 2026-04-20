using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services
{
    public class DkbCsvParserTests
    {
        [Fact]
        public void Parse_ShouldParseSampleCsv()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var relative = Path.Combine(repoRoot, "Finanzuebersicht.Tests", "Services", "test_dkb_sample.csv");
            Assert.True(File.Exists(relative), $"Test CSV not found: {relative}");

            using var fs = File.OpenRead(relative);
            var parser = new DkbCsvParser();
            var txs = parser.Parse(fs).ToList();

            Assert.Equal(4, txs.Count);

            var disney = txs.FirstOrDefault(t => (t.Zahlungsempfaenger ?? string.Empty).Contains("Streaming") || (t.Zahlungsempfaenger ?? string.Empty).Contains("Disney"));
            Assert.NotNull(disney);
            Assert.Equal(-7.99m, disney!.Betrag);

            var salary = txs.FirstOrDefault(t => t.Betrag == 2500.00m);
            Assert.NotNull(salary);
            Assert.Equal("Muster, Max", salary!.Zahlungsempfaenger);
        }

        [Fact]
        public void Parse_ShouldHandleMultilineVerwendungszweck()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var relative = Path.Combine(repoRoot, "Finanzuebersicht.Tests", "Services", "test_dkb_multiline.csv");
            Assert.True(File.Exists(relative), $"Test CSV not found: {relative}");

            using var fs = File.OpenRead(relative);
            var parser = new DkbCsvParser();
            var txs = parser.Parse(fs).ToList();

            Assert.Single(txs);
            var v = txs[0].Verwendungszweck;
            Assert.Contains("Abonnement Linie1", v);
            Assert.Contains("Abonnement Linie2", v);
            Assert.Contains("Zusatzinfo", v);
        }

        [Fact]
        public void Parse_ShouldSkipMalformedRows()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var relative = Path.Combine(repoRoot, "Finanzuebersicht.Tests", "Services", "test_dkb_malformed.csv");
            Assert.True(File.Exists(relative), $"Test CSV not found: {relative}");

            using var fs = File.OpenRead(relative);
            var parser = new DkbCsvParser();
            var txs = parser.Parse(fs).ToList();

            // one malformed line should be skipped, expect 2 valid transactions
            Assert.Equal(2, txs.Count);
            Assert.Contains(txs, t => t.Betrag == -120.00m);
            Assert.Contains(txs, t => t.Betrag == 300.00m);
        }
    }
}
