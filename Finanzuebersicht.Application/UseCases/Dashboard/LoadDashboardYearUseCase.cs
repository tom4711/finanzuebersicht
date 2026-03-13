using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadDashboardYearUseCase(IReportingService reportingService)
{
    private readonly IReportingService _reportingService = reportingService;

    public async Task<DashboardYearData> ExecuteAsync(int year)
    {
        var summary = await _reportingService.GetYearSummaryAsync(year);
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
            {
                cat.PercentageAmount = cat.Total / summary.Total * 100;
            }
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