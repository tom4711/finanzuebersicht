using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

public interface IReportingService
{
    Task<YearSummary> GetYearSummaryAsync(int year);
    Task<MonthSummary> GetMonthSummaryAsync(int year, int month);
}
