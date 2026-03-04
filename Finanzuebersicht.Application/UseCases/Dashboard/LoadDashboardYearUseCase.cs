using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadDashboardYearUseCase
{
    private readonly GetYearSummaryUseCase _getYearSummaryUseCase;

    public LoadDashboardYearUseCase(GetYearSummaryUseCase getYearSummaryUseCase)
    {
        _getYearSummaryUseCase = getYearSummaryUseCase;
    }

    public async Task<DashboardYearData> ExecuteAsync(int year)
    {
        var summary = await _getYearSummaryUseCase.ExecuteAsync(year);
        if (summary == null)
        {
            return new DashboardYearData
            {
                GesamtAusgaben = 0,
                Monate = [],
                Kategorien = []
            };
        }

        if (summary.ByCategory != null && summary.Total > 0)
        {
            foreach (var cat in summary.ByCategory)
                cat.PercentageAmount = (cat.Total / summary.Total) * 100;
        }

        var jahrKatGefiltert = (summary.ByCategory ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
            .ToList();

        return new DashboardYearData
        {
            GesamtAusgaben = summary.Total,
            Monate = summary.Months,
            Kategorien = jahrKatGefiltert
        };
    }
}

public class DashboardYearData
{
    public decimal GesamtAusgaben { get; set; }
    public List<MonthSummary> Monate { get; set; } = [];
    public List<CategorySummary> Kategorien { get; set; } = [];
}