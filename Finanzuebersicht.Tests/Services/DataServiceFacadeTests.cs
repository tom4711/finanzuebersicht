using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class DataServiceFacadeTests
{
    [Fact]
    public async Task GetCategoriesAsync_DelegatesToCategoryRepository()
    {
        var sut = CreateSut(out var categoryRepository, out _, out _, out _, out _);
        var expected = new List<Category> { new() { Id = "cat-1", Name = "Test" } };
        categoryRepository.GetCategoriesAsync().Returns(expected);

        var result = await sut.GetCategoriesAsync();

        Assert.Same(expected, result);
        await categoryRepository.Received(1).GetCategoriesAsync();
    }

    [Fact]
    public async Task SaveCategoryAsync_DelegatesToCategoryRepository()
    {
        var sut = CreateSut(out var categoryRepository, out _, out _, out _, out _);
        var category = new Category { Id = "cat-1", Name = "Miete" };

        await sut.SaveCategoryAsync(category);

        await categoryRepository.Received(1).SaveCategoryAsync(category);
    }

    [Fact]
    public async Task DeleteCategoryAsync_DelegatesToCategoryRepository()
    {
        var sut = CreateSut(out var categoryRepository, out _, out _, out _, out _);

        await sut.DeleteCategoryAsync("cat-1");

        await categoryRepository.Received(1).DeleteCategoryAsync("cat-1");
    }

    [Fact]
    public async Task GetTransactionsAsync_DelegatesToTransactionRepository()
    {
        var sut = CreateSut(out _, out var transactionRepository, out _, out _, out _);
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 1, 31);
        var expected = new List<Transaction>
        {
            new() { Id = "tx-1", Betrag = 100m }
        };
        transactionRepository.GetTransactionsAsync(from, to).Returns(expected);

        var result = await sut.GetTransactionsAsync(from, to);

        Assert.Same(expected, result);
        await transactionRepository.Received(1).GetTransactionsAsync(from, to);
    }

    [Fact]
    public async Task SaveTransactionAsync_DelegatesToTransactionRepository()
    {
        var sut = CreateSut(out _, out var transactionRepository, out _, out _, out _);
        var transaction = new Transaction { Id = "tx-1", Betrag = 25m };

        await sut.SaveTransactionAsync(transaction);

        await transactionRepository.Received(1).SaveTransactionAsync(transaction);
    }

    [Fact]
    public async Task DeleteTransactionAsync_DelegatesToTransactionRepository()
    {
        var sut = CreateSut(out _, out var transactionRepository, out _, out _, out _);

        await sut.DeleteTransactionAsync("tx-1");

        await transactionRepository.Received(1).DeleteTransactionAsync("tx-1");
    }

    [Fact]
    public async Task GetRecurringTransactionsAsync_DelegatesToRecurringRepository()
    {
        var sut = CreateSut(out _, out _, out var recurringRepository, out _, out _);
        var expected = new List<RecurringTransaction>
        {
            new() { Id = "rec-1", Betrag = 20m }
        };
        recurringRepository.GetRecurringTransactionsAsync().Returns(expected);

        var result = await sut.GetRecurringTransactionsAsync();

        Assert.Same(expected, result);
        await recurringRepository.Received(1).GetRecurringTransactionsAsync();
    }

    [Fact]
    public async Task SaveRecurringTransactionAsync_DelegatesToRecurringRepository()
    {
        var sut = CreateSut(out _, out _, out var recurringRepository, out _, out _);
        var recurring = new RecurringTransaction { Id = "rec-1", Betrag = 15m };

        await sut.SaveRecurringTransactionAsync(recurring);

        await recurringRepository.Received(1).SaveRecurringTransactionAsync(recurring);
    }

    [Fact]
    public async Task DeleteRecurringTransactionAsync_DelegatesToRecurringRepository()
    {
        var sut = CreateSut(out _, out _, out var recurringRepository, out _, out _);

        await sut.DeleteRecurringTransactionAsync("rec-1");

        await recurringRepository.Received(1).DeleteRecurringTransactionAsync("rec-1");
    }

    [Fact]
    public async Task GeneratePendingRecurringTransactionsAsync_DelegatesToRecurringGenerationService()
    {
        var facade = CreateSut(out _, out _, out _, out var recurringGenerationService, out _);

        await facade.GeneratePendingRecurringTransactionsAsync();

        await recurringGenerationService.Received(1).GeneratePendingRecurringTransactionsAsync();
    }

    [Fact]
    public async Task GetYearSummaryAsync_DelegatesToReportingService()
    {
        var facade = CreateSut(out _, out _, out _, out _, out var reportingService);

        var expected = new YearSummary { Year = 2025, Total = 123m };
        reportingService.GetYearSummaryAsync(2025).Returns(expected);

        var result = await facade.GetYearSummaryAsync(2025);

        Assert.Same(expected, result);
        await reportingService.Received(1).GetYearSummaryAsync(2025);
    }

    [Fact]
    public async Task GetMonthSummaryAsync_DelegatesToReportingService()
    {
        var facade = CreateSut(out _, out _, out _, out _, out var reportingService);
        var expected = new MonthSummary { Year = 2025, Month = 2, Total = 180m };
        reportingService.GetMonthSummaryAsync(2025, 2).Returns(expected);

        var result = await facade.GetMonthSummaryAsync(2025, 2);

        Assert.Same(expected, result);
        await reportingService.Received(1).GetMonthSummaryAsync(2025, 2);
    }

    private static DataServiceFacade CreateSut(
        out ICategoryRepository categoryRepository,
        out ITransactionRepository transactionRepository,
        out IRecurringTransactionRepository recurringRepository,
        out IRecurringGenerationService recurringGenerationService,
        out IReportingService reportingService)
    {
        categoryRepository = Substitute.For<ICategoryRepository>();
        transactionRepository = Substitute.For<ITransactionRepository>();
        recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        recurringGenerationService = Substitute.For<IRecurringGenerationService>();
        reportingService = Substitute.For<IReportingService>();

        return new DataServiceFacade(
            categoryRepository,
            transactionRepository,
            recurringRepository,
            recurringGenerationService,
            reportingService);
    }
}
