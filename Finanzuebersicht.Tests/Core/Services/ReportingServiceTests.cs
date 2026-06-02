using Finanzuebersicht.Models;

namespace Finanzuebersicht.Tests.Services;

public class ReportingServiceTests
{
    [Fact]
    public async Task GetMonthSummaryAsync_UsesOnlyExpenseTransactions()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "c1", Name = "Essen", Color = "#FF0000", Icon = "🛒" }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Betrag = 25m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 5), AccountId = "acc-1" },
                new() { Betrag = 10m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 6), AccountId = "acc-2" },
                new() { Betrag = 100m, KategorieId = "c1", Typ = TransactionType.Einnahme, Datum = new DateTime(2025, 6, 7), AccountId = "acc-1" }
            });

        var service = new ReportingService(transactionRepository, categoryRepository);

        var summary = await service.GetMonthSummaryAsync(2025, 6);

        Assert.Equal(35m, summary.Total);
        Assert.Single(summary.ByCategory);
        Assert.Equal("Essen", summary.ByCategory[0].CategoryName);
        Assert.Equal(35m, summary.ByCategory[0].Total);
    }

    [Fact]
    public async Task GetYearSummaryAsync_ComputesTotalsAndMonths()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "c1", Name = "Essen", Color = "#FF0000", Icon = "🛒" },
            new() { Id = "c2", Name = "Transport", Color = "#007AFF", Icon = "🚗" }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Betrag = 40m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 1, 3), AccountId = "acc-1" },
                new() { Betrag = 20m, KategorieId = "c2", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 2, 15), AccountId = "acc-2" },
                new() { Betrag = 200m, KategorieId = "c1", Typ = TransactionType.Einnahme, Datum = new DateTime(2025, 3, 1), AccountId = "acc-1" }
            });

        var service = new ReportingService(transactionRepository, categoryRepository);

        var summary = await service.GetYearSummaryAsync(2025);

        Assert.Equal(60m, summary.Total);
        Assert.Equal(12, summary.Months.Count);
        Assert.Equal(2, summary.ByCategory.Count);
        Assert.Contains(summary.ByCategory, c => c.CategoryId == "c1" && c.Total == 40m);
        Assert.Contains(summary.ByCategory, c => c.CategoryId == "c2" && c.Total == 20m);
    }

    [Fact]
    public async Task GetYearSummaryAsync_IgnoresAccountAssignment()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "c1", Name = "Essen", Color = "#FF0000", Icon = "🛒" }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Betrag = 40m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 1, 3), AccountId = "acc-1" },
                new() { Betrag = 20m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 2, 15), AccountId = "acc-2" }
            });

        var service = new ReportingService(transactionRepository, categoryRepository);

        var summary = await service.GetYearSummaryAsync(2025);

        Assert.Equal(60m, summary.Total);
        Assert.Single(summary.ByCategory);
        Assert.Equal("Essen", summary.ByCategory[0].CategoryName);
    }

    [Fact]
    public async Task GetMonthSummaryAsync_ExcludesTransfers()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "c1", Name = "Essen", Color = "#FF0000", Icon = "🛒" }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Betrag = 25m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 5), IsTransfer = false },
                new() { Betrag = 40m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 6), IsTransfer = true }
            });

        var service = new ReportingService(transactionRepository, categoryRepository);

        var summary = await service.GetMonthSummaryAsync(2025, 6);

        Assert.Equal(25m, summary.Total);
    }
}
