using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;

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
                new() { Typ = TransactionType.Einnahme, Betrag = 2000m, KategorieId = "cat-a", AccountId = "acc-1" },
                new() { Typ = TransactionType.Ausgabe, Betrag = 800m, KategorieId = "cat-b", AccountId = "acc-2" }
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

    [Fact]
    public async Task ExecuteAsync_CurrentMonthComputesBudgetHints()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Ausgabe, Betrag = 150m, KategorieId = "cat-food" }
            });
        budgetRepository.GetBudgetsAsync().Returns(new List<CategoryBudget>
        {
            new() { Id = "budget-food", KategorieId = "cat-food", Betrag = 300m }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15));

        var hint = Assert.Single(result.BudgetHinweise);
        Assert.Equal(300m, hint.BudgetBetrag);
        Assert.Equal(150m, hint.Verbrauch);
        Assert.Equal(150m, hint.Restbudget);
        Assert.Equal(17, hint.VerbleibendeTage);
        Assert.Equal(150m / 17m, hint.Tagesbudget);
        Assert.False(hint.IstWarnung);
        Assert.False(hint.IstAusgeschoepft);
    }

    [Theory]
    [InlineData(240, true, false)]
    [InlineData(300, false, true)]
    [InlineData(330, false, true)]
    public async Task ExecuteAsync_CurrentMonthAppliesFixedBudgetThresholds(decimal spent, bool warning, bool exceeded)
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Ausgabe, Betrag = spent, KategorieId = "cat-food" }
            });
        budgetRepository.GetBudgetsAsync().Returns(new List<CategoryBudget>
        {
            new() { Id = "budget-food", KategorieId = "cat-food", Betrag = 300m }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15));

        var hint = Assert.Single(result.BudgetHinweise);
        Assert.Equal(warning, hint.IstWarnung);
        Assert.Equal(exceeded, hint.IstAusgeschoepft);
    }

    [Fact]
    public async Task ExecuteAsync_BudgetedCategoryWithoutSpendingAppearsOnlyInBudgetHints()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>());
        budgetRepository.GetBudgetsAsync().Returns(new List<CategoryBudget>
        {
            new() { Id = "budget-food", KategorieId = "cat-food", Betrag = 300m }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15));

        Assert.Empty(result.KategorieAusgaben);
        var hint = Assert.Single(result.BudgetHinweise);
        Assert.Equal("cat-food", hint.CategoryId);
        Assert.Equal(0m, hint.Verbrauch);
    }

    [Fact]
    public async Task ExecuteAsync_PastMonthDoesNotExposeActionOrDailyBudgetHints()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Ausgabe, Betrag = 300m, KategorieId = "cat-food" }
            });
        budgetRepository.GetBudgetsAsync().Returns(new List<CategoryBudget>
        {
            new() { Id = "budget-food", KategorieId = "cat-food", Betrag = 300m }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 2, 1), new DateTime(2026, 3, 15));

        var hint = Assert.Single(result.BudgetHinweise);
        Assert.False(hint.IstAktuellerMonat);
        Assert.False(hint.ZeigeTagesbudget);
        Assert.Equal(0m, hint.Tagesbudget);
        Assert.False(hint.IstWarnung);
        Assert.False(hint.IstAusgeschoepft);
    }

    [Fact]
    public async Task ExecuteAsync_ForecastRecurring_AppliesAccountFilter()
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
                AccountId = "acc-1",
                Typ = TransactionType.Ausgabe,
                Aktiv = true,
                Startdatum = new DateTime(2025, 1, 1)
            },
            new()
            {
                Id = "rec-2",
                Titel = "Gym",
                Betrag = 50m,
                KategorieId = "cat-a",
                AccountId = "acc-2",
                Typ = TransactionType.Ausgabe,
                Aktiv = true,
                Startdatum = new DateTime(2025, 1, 1)
            }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, Substitute.For<IBudgetRepository>());

        var result = await useCase.ExecuteAsync(new DateTime(2026, 4, 1), new DateTime(2026, 3, 15), "acc-1");

        Assert.True(result.IstPrognose);
        Assert.Equal(15m, result.GesamtAusgaben);
        Assert.Single(result.KategorieAusgaben);
        Assert.Equal(15m, result.KategorieAusgaben[0].Total);
    }

    [Fact]
    public async Task ExecuteAsync_ExcludesTransfers_AndAppliesAccountFilter()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Ausgabe, Betrag = 100m, KategorieId = "cat-food", AccountId = "acc-1", IsTransfer = false },
                new() { Typ = TransactionType.Ausgabe, Betrag = 40m, KategorieId = "cat-food", AccountId = "acc-1", IsTransfer = true },
                new() { Typ = TransactionType.Ausgabe, Betrag = 30m, KategorieId = "cat-food", AccountId = "acc-2", IsTransfer = false }
            });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15), "acc-1");

        Assert.Equal(100m, result.GesamtAusgaben);
        Assert.Single(result.KategorieAusgaben);
        Assert.Equal(100m, result.KategorieAusgaben[0].Total);
    }

    [Fact]
    public async Task ExecuteAsync_BudgetHinweise_AppliesAccountFilter()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();

        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-food", Name = "Food", Typ = TransactionType.Ausgabe }
        });
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Typ = TransactionType.Ausgabe, Betrag = 80m, KategorieId = "cat-food", AccountId = "acc-1" },
                new() { Typ = TransactionType.Ausgabe, Betrag = 200m, KategorieId = "cat-food", AccountId = "acc-2" }
            });
        budgetRepository.GetBudgetsAsync().Returns(new List<CategoryBudget>
        {
            new() { Id = "budget-food", KategorieId = "cat-food", Betrag = 300m }
        });

        var useCase = new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringRepository, budgetRepository);

        var result = await useCase.ExecuteAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 15), "acc-1");

        var hint = Assert.Single(result.BudgetHinweise);
        Assert.Equal(80m, hint.Verbrauch);
        Assert.Equal(220m, hint.Restbudget);
    }
}