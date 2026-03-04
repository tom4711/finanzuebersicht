using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class GetYearSummaryUseCase
{
    private readonly IReportingService _reportingService;

    public GetYearSummaryUseCase(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public Task<YearSummary> ExecuteAsync(int year)
        => _reportingService.GetYearSummaryAsync(year);
}