using Finanzuebersicht.Services;
using Finanzuebersicht.Models;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Tests.Services
{
    public class ImportServiceTests
    {
        [Fact]
        public async Task ImportFromCsv_ShouldUseParserAndPersistTransactions()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "A" },
                new() { Buchungsdatum = DateTime.Today, Betrag = -5m, Zahlungsempfaenger = "B" }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService([parser], repo, logger);

            using var ms = new MemoryStream();
            var result = await svc.ImportFromCsvAsync(ms);

            Assert.Equal(2, result.ToList().Count);
            await repo.Received(2).SaveTransactionAsync(Arg.Any<Transaction>());
        }
    }
}
