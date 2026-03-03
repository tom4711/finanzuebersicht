using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface IReportingService
{
    Task<YearSummary> GetYearSummaryAsync(int year);
    Task<MonthSummary> GetMonthSummaryAsync(int year, int month);
}
