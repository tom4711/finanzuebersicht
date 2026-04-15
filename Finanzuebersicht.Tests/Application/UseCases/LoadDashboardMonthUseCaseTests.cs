using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadDashboardMonthUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComputesMonthlyTotalsAndCategories()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-a", Name = "Gehalt", Typ = TransactionType.Einnahme },
            new() { Id = "cat-b", Name = "Miete", Typ = TransactionType.Ausgabe }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Einnahme, Betrag = 2000m, KategorieId = "cat-a" },
                new() { Typ = TransactionType.Ausgabe, Betrag = 800m, KategorieId = "cat-b" }
            });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, Substitute.For<IBudgetRepository>());

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15));

        Assert.False(result.IstPrognose);
        Assert.Equal(2000m, result.GesamtEinnahmen);
        Assert.Equal(800m, result.GesamtAusgaben);
        Assert.Equal(1200m, result.Bilanz);
        Assert.Single(result.KategorieEinnahmen);
        Assert.Single(result.KategorieAusgaben);
        await recurringRepository.DidNotReceiveWithAnyArgs().GetRecurringTransactionsAsync();
    }

    [Fact]
    public async Task ExecuteAsync_AddsForecastRecurringTransactionsForFutureMonths()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-a", Name = "Abo", Typ = TransactionType.Ausgabe }
        });

        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>());

        recurringRepository.GetRecurringTransactionsAsync().Returns(new List<RecurringTransaction>
        {
            new()
            {
                Id = "rec-1",
                Titel = "Streaming",
                Betrag = 15m,
                KategorieId = "cat-a",
                Typ = TransactionType.Ausgabe,
                Aktiv = true,
                Startdatum = new DateTime(2025, 1, 1)
            }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, Substitute.For<IBudgetRepository>());

        var result = await useCase.ExecuteAsync(new DateTime(2026, 4, 1), new DateTime(2026, 3, 15));

        Assert.True(result.IstPrognose);
        Assert.Equal(0m, result.GesamtEinnahmen);
        Assert.Equal(15m, result.GesamtAusgaben);
        Assert.Equal(-15m, result.Bilanz);
        Assert.Single(result.KategorieAusgaben);
        await recurringRepository.Received(1).GetRecurringTransactionsAsync();
    }
}