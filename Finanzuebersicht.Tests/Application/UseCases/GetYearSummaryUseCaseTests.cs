using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class GetYearSummaryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToReportingService()
    {
        var reportingService = Substitute.For<IReportingService>();
        var expected = new YearSummary { Year = 2026, Total = 999m };
        reportingService.GetYearSummaryAsync(2026).Returns(expected);
        var useCase = new GetYearSummaryUseCase(reportingService);

        var result = await useCase.ExecuteAsync(2026);

        Assert.Same(expected, result);
        await reportingService.Received(1).GetYearSummaryAsync(2026);
    }
}