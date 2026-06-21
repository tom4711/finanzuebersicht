using System.Globalization;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using Finanzuebersicht.Presentation.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.Presentation.Accessibility;

public static class ChartAccessibilitySummaryBuilder
{
    public static string BuildCategoryDonutSummary(
        IEnumerable<CategorySummary> items,
        ILocalizationService loc,
        CultureInfo culture)
    {
        var list = items.Where(i => i.Total != 0).OrderByDescending(i => i.Total).ToList();
        if (list.Count == 0)
            return loc.GetString(ResourceKeys.A11y_ChartLeer);

        var segments = list
            .Take(6)
            .Select(i => loc.GetString(
                ResourceKeys.A11y_ChartCategorySegment,
                i.CategoryName,
                i.Total.ToString("C", culture),
                i.PercentageDisplay));

        var summary = string.Join("; ", segments);
        if (list.Count > 6)
            summary += "; " + loc.GetString(ResourceKeys.A11y_ChartUndWeitere, list.Count - 6);

        return summary;
    }

    public static string BuildMonthBarSummary(
        IEnumerable<MonthSummary> months,
        ILocalizationService loc,
        CultureInfo culture,
        int forecastMonth = 0,
        decimal forecastValue = 0)
    {
        var list = months.OrderBy(m => m.Month).ToList();
        if (list.Count == 0 && forecastMonth <= 0)
            return loc.GetString(ResourceKeys.A11y_ChartLeer);

        var segments = list.Select(m =>
        {
            var monthName = culture.DateTimeFormat.GetAbbreviatedMonthName(m.Month);
            return loc.GetString(
                ResourceKeys.A11y_ChartMonthSegment,
                monthName,
                m.Total.ToString("C", culture));
        }).ToList();

        if (forecastMonth > 0 && forecastValue > 0)
        {
            var monthName = culture.DateTimeFormat.GetAbbreviatedMonthName(forecastMonth);
            segments.Add(loc.GetString(
                ResourceKeys.A11y_ChartForecastSegment,
                monthName,
                forecastValue.ToString("C", culture)));
        }

        return string.Join("; ", segments);
    }
}
