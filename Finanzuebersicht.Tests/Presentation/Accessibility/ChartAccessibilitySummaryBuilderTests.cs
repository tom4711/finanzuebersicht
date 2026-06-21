using System.Globalization;
using Finanzuebersicht.Models;
using Finanzuebersicht.Presentation.Accessibility;
using Finanzuebersicht.Presentation.Services;
using Finanzuebersicht.Resources.Strings;
using NSubstitute;

namespace Finanzuebersicht.Tests.Presentation.Accessibility;

public class ChartAccessibilitySummaryBuilderTests
{
    [Fact]
    public void BuildCategoryDonutSummary_Empty_ReturnsEmptyLabel()
    {
        var loc = Substitute.For<ILocalizationService>();
        loc.GetString(ResourceKeys.A11y_ChartLeer).Returns("Keine Daten");

        var summary = ChartAccessibilitySummaryBuilder.BuildCategoryDonutSummary([], loc, CultureInfo.GetCultureInfo("de-DE"));

        Assert.Equal("Keine Daten", summary);
    }

    [Fact]
    public void BuildCategoryDonutSummary_FormatsTopCategories()
    {
        var loc = Substitute.For<ILocalizationService>();
        loc.GetString(ResourceKeys.A11y_ChartCategorySegment, Arg.Any<object[]>())
            .Returns(call => $"{call.ArgAt<object[]>(1)[0]} {call.ArgAt<object[]>(1)[1]} {call.ArgAt<object[]>(1)[2]}");

        var items = new[]
        {
            new CategorySummary { CategoryName = "Wohnen", Total = 720m, PercentageAmount = 72m },
            new CategorySummary { CategoryName = "Essen", Total = 180m, PercentageAmount = 18m }
        };

        var summary = ChartAccessibilitySummaryBuilder.BuildCategoryDonutSummary(items, loc, CultureInfo.GetCultureInfo("de-DE"));

        Assert.Contains("Wohnen", summary);
        Assert.Contains("Essen", summary);
    }

    [Fact]
    public void BuildMonthBarSummary_IncludesForecastWhenProvided()
    {
        var loc = Substitute.For<ILocalizationService>();
        loc.GetString(ResourceKeys.A11y_ChartMonthSegment, Arg.Any<object[]>())
            .Returns(call => $"{call.ArgAt<object[]>(1)[0]} {call.ArgAt<object[]>(1)[1]}");
        loc.GetString(ResourceKeys.A11y_ChartForecastSegment, Arg.Any<object[]>())
            .Returns("Prognose");

        var months = new List<MonthSummary>
        {
            new() { Month = 1, Total = 100m },
            new() { Month = 2, Total = 200m }
        };

        var summary = ChartAccessibilitySummaryBuilder.BuildMonthBarSummary(
            months, loc, CultureInfo.GetCultureInfo("de-DE"), forecastMonth: 3, forecastValue: 300m);

        Assert.Contains("Prognose", summary);
    }
}
