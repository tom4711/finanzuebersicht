using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

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
                new() { Betrag = 25m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 5) },
                new() { Betrag = 10m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 6, 6) },
                new() { Betrag = 100m, KategorieId = "c1", Typ = TransactionType.Einnahme, Datum = new DateTime(2025, 6, 7) }
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
                new() { Betrag = 40m, KategorieId = "c1", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 1, 3) },
                new() { Betrag = 20m, KategorieId = "c2", Typ = TransactionType.Ausgabe, Datum = new DateTime(2025, 2, 15) },
                new() { Betrag = 200m, KategorieId = "c1", Typ = TransactionType.Einnahme, Datum = new DateTime(2025, 3, 1) }
            });

        var service = new ReportingService(transactionRepository, categoryRepository);

        var summary = await service.GetYearSummaryAsync(2025);

        Assert.Equal(60m, summary.Total);
        Assert.Equal(12, summary.Months.Count);
        Assert.Equal(2, summary.ByCategory.Count);
        Assert.Contains(summary.ByCategory, c => c.CategoryId == "c1" && c.Total == 40m);
        Assert.Contains(summary.ByCategory, c => c.CategoryId == "c2" && c.Total == 20m);
    }
}
