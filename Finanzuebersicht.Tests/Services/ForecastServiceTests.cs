using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class ForecastServiceTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IBudgetRepository _budgetRepository = Substitute.For<IBudgetRepository>();

    private ForecastService CreateService() =>
        new(_transactionRepository, _categoryRepository, _budgetRepository);

    public ForecastServiceTests()
    {
        _categoryRepository.GetCategoriesAsync().Returns([]);
        _budgetRepository.GetBudgetsAsync().Returns([]);
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);
    }

    [Fact]
    public async Task EmptyHistory_ReturnsForecastTotalZero()
    {
        var service = CreateService();

        var result = await service.GetMovingAverageAsync(2025, 4);

        Assert.Equal(0m, result.ForecastedTotal);
        Assert.Empty(result.ByCategory);
        Assert.Equal(2025, result.Year);
        Assert.Equal(4, result.Month);
    }

    [Fact]
    public async Task ConsistentSpending_ReturnsCorrectAverage()
    {
        _categoryRepository.GetCategoriesAsync().Returns([
            new Category { Id = "food", Name = "Food", Typ = TransactionType.Ausgabe }
        ]);

        // 3 months each with 300 spending = average 300
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([
                new Transaction { Typ = TransactionType.Ausgabe, Betrag = 300m, KategorieId = "food" }
            ]);

        var service = CreateService();
        var result = await service.GetMovingAverageAsync(2025, 4, lookbackMonths: 3);

        Assert.Equal(300m, result.ForecastedTotal);
        Assert.Single(result.ByCategory);
        Assert.Equal("Food", result.ByCategory[0].CategoryName);
        Assert.Equal(300m, result.ByCategory[0].Total);
    }

    [Fact]
    public async Task VaryingSpending_ReturnsWeightedAverage()
    {
        _categoryRepository.GetCategoriesAsync().Returns([
            new Category { Id = "food", Name = "Food", Typ = TransactionType.Ausgabe }
        ]);

        // Month -3: 100, Month -2: 200, Month -1: 300 → average = 600/3 = 200
        var callCount = 0;
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(_ =>
            {
                callCount++;
                return callCount switch
                {
                    1 => [new Transaction { Typ = TransactionType.Ausgabe, Betrag = 100m, KategorieId = "food" }],
                    2 => [new Transaction { Typ = TransactionType.Ausgabe, Betrag = 200m, KategorieId = "food" }],
                    3 => [new Transaction { Typ = TransactionType.Ausgabe, Betrag = 300m, KategorieId = "food" }],
                    _ => []
                };
            });

        var service = CreateService();
        var result = await service.GetMovingAverageAsync(2025, 4, lookbackMonths: 3);

        Assert.Equal(200m, result.ForecastedTotal);
    }

    [Fact]
    public async Task LookbackMonths_RespectedInCalculation()
    {
        _categoryRepository.GetCategoriesAsync().Returns([
            new Category { Id = "cat", Name = "Cat", Typ = TransactionType.Ausgabe }
        ]);

        // With lookbackMonths=2, only 2 calls should be made
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([
                new Transaction { Typ = TransactionType.Ausgabe, Betrag = 120m, KategorieId = "cat" }
            ]);

        var service = CreateService();
        var result = await service.GetMovingAverageAsync(2025, 4, lookbackMonths: 2);

        // 120 total over 2 months but same each month = 120/2 = 60 per call, total 240/2 = 120
        // Actually: 2 calls each returning 120, summed = 240, divided by lookbackMonths(2) = 120
        Assert.Equal(120m, result.ForecastedTotal);
        Assert.Equal(2, result.LookbackMonths);

        await _transactionRepository.Received(2).GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task EnrichesWithBudget_WhenBudgetExists()
    {
        _categoryRepository.GetCategoriesAsync().Returns([
            new Category { Id = "food", Name = "Food", Typ = TransactionType.Ausgabe }
        ]);
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([
                new Transaction { Typ = TransactionType.Ausgabe, Betrag = 200m, KategorieId = "food" }
            ]);
        _budgetRepository.GetBudgetForCategoryAsync("food", 2025, 4)
            .Returns(new CategoryBudget { KategorieId = "food", Betrag = 300m, Monat = null, Jahr = null });

        var service = CreateService();
        var result = await service.GetMovingAverageAsync(2025, 4, lookbackMonths: 1);

        Assert.Single(result.ByCategory);
        Assert.Equal(300m, result.ByCategory[0].BudgetBetrag);
        Assert.True(result.ByCategory[0].HatBudget);
    }

    [Fact]
    public async Task IncomeTransactions_NotIncludedInForecast()
    {
        _categoryRepository.GetCategoriesAsync().Returns([
            new Category { Id = "salary", Name = "Salary", Typ = TransactionType.Einnahme }
        ]);
        _transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns([
                new Transaction { Typ = TransactionType.Einnahme, Betrag = 3000m, KategorieId = "salary" }
            ]);

        var service = CreateService();
        var result = await service.GetMovingAverageAsync(2025, 4);

        Assert.Equal(0m, result.ForecastedTotal);
        Assert.Empty(result.ByCategory);
    }
}
