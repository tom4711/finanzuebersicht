using System.IO;
using System.Linq;
using Xunit;
using Finanzuebersicht.Core.Services;

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

            var disney = txs.FirstOrDefault(t => (t.Titel ?? string.Empty).Contains("Streaming") || (t.Titel ?? string.Empty).Contains("Disney"));
            Assert.NotNull(disney);
            Assert.Equal(-7.99m, disney!.Betrag);

            var salary = txs.FirstOrDefault(t => t.Betrag == 2500.00m);
            Assert.NotNull(salary);
            Assert.Equal("Muster, Max", salary!.Titel);
        }
    }
}
