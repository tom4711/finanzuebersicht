using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.UseCases;

public class GetMonthSummaryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToReportingService()
    {
        var reportingService = Substitute.For<IReportingService>();
        var expected = new MonthSummary { Year = 2026, Month = 3, Total = 123m };
        reportingService.GetMonthSummaryAsync(2026, 3).Returns(expected);
        var useCase = new GetMonthSummaryUseCase(reportingService);

        var result = await useCase.ExecuteAsync(2026, 3);

        Assert.Same(expected, result);
        await reportingService.Received(1).GetMonthSummaryAsync(2026, 3);
    }
}