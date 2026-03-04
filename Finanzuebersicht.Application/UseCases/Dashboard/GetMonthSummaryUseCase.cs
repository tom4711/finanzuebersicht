using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class GetMonthSummaryUseCase
{
    private readonly IReportingService _reportingService;

    public GetMonthSummaryUseCase(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public Task<MonthSummary> ExecuteAsync(int year, int month)
        => _reportingService.GetMonthSummaryAsync(year, month);
}