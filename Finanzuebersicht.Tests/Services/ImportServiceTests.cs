using System.IO;
using System.Linq;
using NSubstitute;
using Xunit;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Services;

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
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = 10m, Verwendungszweck = "A" },
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = -5m, Verwendungszweck = "B" }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new [] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Equal(2, result.Count);
            repo.Received(2).SaveTransactionAsync(Arg.Any<Transaction>());
        }

        [Fact]
        public void ImportFromCsv_WithEmptyStream_ReturnsEmpty()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            parser.Parse(Arg.Any<Stream>()).Returns(Enumerable.Empty<TransactionDto>());

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new[] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Empty(result);
            repo.DidNotReceive().SaveTransactionAsync(Arg.Any<Transaction>());
        }

        [Fact]
        public void ImportFromCsv_WithMultipleParsers_UsesFirstMatchingParser()
        {
            var parser1 = Substitute.For<IStatementParser>();
            var parser2 = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = 50m, Verwendungszweck = "Parser2" }
            };

            parser1.Parse(Arg.Any<Stream>()).Returns(Enumerable.Empty<TransactionDto>());
            parser2.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new[] { parser1, parser2 }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Single(result);
            Assert.Equal("Parser2", result[0].Verwendungszweck);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(10000)]
        public void ImportFromCsv_WithVariousAmounts_PersistsCorrectly(decimal amount)
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = amount, Verwendungszweck = "Test" }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new[] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Single(result);
            Assert.Equal(amount, result[0].Betrag);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Short")]
        [InlineData("This is a very long description that should still be handled correctly")]
        public void ImportFromCsv_WithVariousDescriptions_PersistsCorrectly(string description)
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new TransactionDto { Buchungsdatum = System.DateTime.Today, Betrag = 10m, Verwendungszweck = description }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new[] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Single(result);
            Assert.Equal(description, result[0].Verwendungszweck);
        }

        [Fact]
        public void ImportFromCsv_PastAndFutureDates_BothHandledCorrectly()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var pastDate = System.DateTime.Today.AddDays(-30);
            var futureDate = System.DateTime.Today.AddDays(30);

            var dtos = new List<TransactionDto>
            {
                new TransactionDto { Buchungsdatum = pastDate, Betrag = 10m, Verwendungszweck = "Past" },
                new TransactionDto { Buchungsdatum = futureDate, Betrag = 20m, Verwendungszweck = "Future" }
            };

            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService(new[] { parser }, repo, logger);

            using var ms = new MemoryStream();
            var result = svc.ImportFromCsv(ms).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(pastDate.Date, result[0].Datum.Date);
            Assert.Equal(futureDate.Date, result[1].Datum.Date);
        }
    }
}
