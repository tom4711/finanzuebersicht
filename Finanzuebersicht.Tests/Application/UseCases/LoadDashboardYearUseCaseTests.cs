using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadDashboardYearUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredCategoriesAndPercentages()
    {
        var reportingService = Substitute.For<IReportingService>();
        reportingService.GetYearSummaryAsync(2026).Returns(new YearSummary
        {
            Total = 100m,
            Months = new List<MonthSummary> { new() { Month = 1, Total = 40m }, new() { Month = 2, Total = 60m } },
            ByCategory = new List<CategorySummary>
            {
                new() { CategoryId = "cat-1", CategoryName = "Miete", Total = 60m },
                new() { CategoryId = "cat-2", CategoryName = "", Total = 40m }
            }
        });

        var getYearSummaryUseCase = new GetYearSummaryUseCase(reportingService);
        var sut = new LoadDashboardYearUseCase(getYearSummaryUseCase);

        var result = await sut.ExecuteAsync(2026);

        Assert.Equal(100m, result.GesamtAusgaben);
        Assert.Equal(2, result.Monate.Count);
        Assert.Single(result.Kategorien);
        Assert.Equal("cat-1", result.Kategorien[0].CategoryId);
        Assert.Equal(60m, result.Kategorien[0].PercentageAmount);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyData_WhenSummaryIsNull()
    {
        var reportingService = Substitute.For<IReportingService>();
        reportingService.GetYearSummaryAsync(2026).Returns(Task.FromResult((YearSummary)null!));

        var getYearSummaryUseCase = new GetYearSummaryUseCase(reportingService);
        var sut = new LoadDashboardYearUseCase(getYearSummaryUseCase);

        var result = await sut.ExecuteAsync(2026);

        Assert.Equal(0m, result.GesamtAusgaben);
        Assert.Empty(result.Monate);
        Assert.Empty(result.Kategorien);
    }
}