using System.IO;
using System.Linq;
using NSubstitute;
using Xunit;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Tests.Services
{
    public class ImportServiceTests
    {
        [Fact]
        public void ImportFromCsv_ShouldUseParserAndPersistTransactions()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "A" },
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = -5m, Zahlungsempfaenger = "B" }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new [] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Equal(2, result.Count);
            repo.Received(2).SaveTransactionAsync(Arg.Any<Transaction>());
        }
    }
}
