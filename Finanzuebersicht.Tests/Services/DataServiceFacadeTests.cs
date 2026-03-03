using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class DataServiceFacadeTests
{
    [Fact]
    public async Task GeneratePendingRecurringTransactionsAsync_DelegatesToRecurringGenerationService()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var recurringGenerationService = Substitute.For<IRecurringGenerationService>();
        var reportingService = Substitute.For<IReportingService>();

        var facade = new DataServiceFacade(
            categoryRepository,
            transactionRepository,
            recurringRepository,
            recurringGenerationService,
            reportingService);

        await facade.GeneratePendingRecurringTransactionsAsync();

        await recurringGenerationService.Received(1).GeneratePendingRecurringTransactionsAsync();
    }

    [Fact]
    public async Task GetYearSummaryAsync_DelegatesToReportingService()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var recurringGenerationService = Substitute.For<IRecurringGenerationService>();
        var reportingService = Substitute.For<IReportingService>();

        var expected = new YearSummary { Year = 2025, Total = 123m };
        reportingService.GetYearSummaryAsync(2025).Returns(expected);

        var facade = new DataServiceFacade(
            categoryRepository,
            transactionRepository,
            recurringRepository,
            recurringGenerationService,
            reportingService);

        var result = await facade.GetYearSummaryAsync(2025);

        Assert.Same(expected, result);
    }
}
