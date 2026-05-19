using Xunit;
using Finanzuebersicht.Models;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace Finanzuebersicht.Tests.Services
{
    public class ImportServiceTests
    {
        private static ImportService BuildService(
            IStatementParser parser,
            ITransactionRepository repo,
            ICategoryRepository? catRepo = null)
        {
            var logger = Substitute.For<ILogger<ImportService>>();
            return new ImportService([parser], repo, logger, catRepo);
        }

        [Fact]
        public async Task ImportFromCsv_ValidRecords_ImportedAndSaved()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();
            repo.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns([]);

            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "A" },
                new() { Buchungsdatum = DateTime.Today, Betrag = -5m, Zahlungsempfaenger = "B" }
            };
            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var svc = BuildService(parser, repo);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.True(result.Success);
            Assert.Equal(2, result.Imported.Count);
            Assert.Empty(result.Duplicates);
            Assert.Equal(0, result.SkippedMalformed);
            Assert.Empty(result.SaveErrors);
            await repo.Received(2).SaveTransactionAsync(Arg.Any<Transaction>());
        }

        [Fact]
        public async Task ImportFromCsv_DuplicateDetected_SkippedNotSaved()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();

            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "Shop" }
            };
            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            // Simulate existing transaction that matches
            var existing = new Transaction { Betrag = 10m, Titel = "Shop", Datum = DateTime.Today };
            repo.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns([existing]);

            var svc = BuildService(parser, repo);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.True(result.Success);
            Assert.Empty(result.Imported);
            Assert.Single(result.Duplicates);
            await repo.DidNotReceive().SaveTransactionAsync(Arg.Any<Transaction>());
        }

        [Fact]
        public async Task ImportFromCsv_MalformedDto_Skipped()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();
            repo.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns([]);

            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = default, Betrag = 5m }, // malformed: no date
                new() { Buchungsdatum = DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "Valid" }
            };
            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var svc = BuildService(parser, repo);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.True(result.Success);
            Assert.Single(result.Imported);
            Assert.Equal(1, result.SkippedMalformed);
        }

        [Fact]
        public async Task ImportFromCsv_SaveFails_ReportedInSaveErrors()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();
            repo.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns([]);
            repo.SaveTransactionAsync(Arg.Any<Transaction>())
                .Returns(Task.FromException(new IOException("disk full")));

            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = DateTime.Today, Betrag = 10m, Zahlungsempfaenger = "A" }
            };
            parser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var svc = BuildService(parser, repo);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.True(result.Success); // no top-level error
            Assert.Empty(result.Imported);
            Assert.Single(result.SaveErrors);
            Assert.Contains("disk full", result.SaveErrors[0]);
        }

        [Fact]
        public async Task ImportFromCsv_NoParserMatches_ErrorMessage()
        {
            var parser = Substitute.For<IStatementParser>();
            parser.Parse(Arg.Any<Stream>()).Returns([]);
            var repo = Substitute.For<ITransactionRepository>();

            var svc = BuildService(parser, repo);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ImportFromCsv_ParserThrows_TriesNextParser()
        {
            var failingParser = Substitute.For<IStatementParser>();
            failingParser.Parse(Arg.Any<Stream>()).Throws(new Exception("parse error"));

            var workingParser = Substitute.For<IStatementParser>();
            var dtos = new List<TransactionDto>
            {
                new() { Buchungsdatum = DateTime.Today, Betrag = 5m, Zahlungsempfaenger = "OK" }
            };
            workingParser.Parse(Arg.Any<Stream>()).Returns(dtos);

            var repo = Substitute.For<ITransactionRepository>();
            repo.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
                .Returns([]);

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService([failingParser, workingParser], repo, logger);
            using var ms = new MemoryStream();

            var result = await svc.ImportFromCsvAsync(ms);

            Assert.True(result.Success);
            Assert.Single(result.Imported);
        }

        [Fact]
        public async Task ImportFromCsv_Cancellation_ThrowsOperationCancelled()
        {
            var parser = Substitute.For<IStatementParser>();
            var repo = Substitute.For<ITransactionRepository>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var logger = Substitute.For<ILogger<ImportService>>();
            var svc = new ImportService([parser], repo, logger);
            using var ms = new MemoryStream();

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => svc.ImportFromCsvAsync(ms, cancellationToken: cts.Token));
        }
    }
}
