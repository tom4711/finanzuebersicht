using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadDashboardYearUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredCategoriesAndPercentages()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Miete" },
            new() { Id = "cat-2", Name = "Mobilität" }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Transaction>
        {
            new() { KategorieId = "cat-1", Typ = TransactionType.Ausgabe, Betrag = 60m, Datum = new DateTime(2026, 1, 5), AccountId = "acc-1" },
            new() { KategorieId = "cat-2", Typ = TransactionType.Ausgabe, Betrag = 40m, Datum = new DateTime(2026, 2, 3), AccountId = "acc-1" },
            new() { KategorieId = "cat-2", Typ = TransactionType.Ausgabe, Betrag = 20m, Datum = new DateTime(2026, 2, 8), AccountId = "acc-2", IsTransfer = true }
        });

        var sut = new LoadDashboardYearUseCase(transactionRepository, categoryRepository);

        var result = await sut.ExecuteAsync(2026, "acc-1");

        Assert.Equal(100m, result.GesamtAusgaben);
        Assert.Equal(12, result.Monate.Count);
        Assert.Equal(2, result.Kategorien.Count);
        Assert.Contains(result.Kategorien, c => c.CategoryId == "cat-1" && c.Total == 60m);
        Assert.Contains(result.Kategorien, c => c.CategoryId == "cat-2" && c.Total == 40m);
    }

    [Fact]
    public async Task ExecuteAsync_ExcludesTransfers()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Miete" }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Transaction>
        {
            new() { KategorieId = "cat-1", Typ = TransactionType.Ausgabe, Betrag = 120m, Datum = new DateTime(2026, 1, 5), IsTransfer = true },
            new() { KategorieId = "cat-1", Typ = TransactionType.Ausgabe, Betrag = 80m, Datum = new DateTime(2026, 1, 8), IsTransfer = false }
        });

        var sut = new LoadDashboardYearUseCase(transactionRepository, categoryRepository);

        var result = await sut.ExecuteAsync(2026);

        Assert.Equal(80m, result.GesamtAusgaben);
        Assert.Single(result.Kategorien);
        Assert.Equal(80m, result.Kategorien[0].Total);
    }
}