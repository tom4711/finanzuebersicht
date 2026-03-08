using System.Globalization;
using Xunit;

namespace Finanzuebersicht.Tests.Services
{
    public class DkbCsvParserTests
    {
        [Fact]
        public void Parse_ShouldReadAllTransactions_FromSampleCsv()
        {
            var path = "Services/test_dkb_sample.csv";
            var full = System.IO.Path.Combine("Finanzuebersicht.Tests", path);
            Assert.True(System.IO.File.Exists(full), $"Test CSV not found: {full}");

            // Placeholder: actual parser not implemented yet
            var content = System.IO.File.ReadAllText(full);
            Assert.Contains("Buchungsdatum", content);
            Assert.Contains("Disney+ Abonnement", content);
        }
    }
}
