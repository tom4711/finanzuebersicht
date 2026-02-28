using System;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using Finanzuebersicht.Services;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        public YearOverviewViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Year = DateTime.Now.Year;
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SelectCategoryCommand = new RelayCommand<string?>(SelectCategory);

            // keep empty series until data is loaded
            PieSeries = Array.Empty<ISeries>();
            BarSeries = Array.Empty<ISeries>();
            Categories = new List<CategorySummary>();
        }

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private ISeries[] pieSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] barSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private List<CategorySummary> categories;

        [ObservableProperty]
        private string? selectedCategoryId;

        public IAsyncRelayCommand LoadCommand { get; }
        public IRelayCommand<string?> SelectCategoryCommand { get; }

        private async Task LoadAsync()
        {
            var summary = await _dataService.GetYearSummaryAsync(Year);
            Categories = summary.ByCategory;
            UpdateSeries(summary, SelectedCategoryId);
        }

        private void SelectCategory(string? categoryId)
        {
            SelectedCategoryId = categoryId;
            // reload year summary and update series filtered by category
            var summaryTask = _dataService.GetYearSummaryAsync(Year);
            summaryTask.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    UpdateSeries(t.Result, categoryId);
            });
        }

        private void UpdateSeries(YearSummary summary, string? categoryId)
        {
            // Pie: always show by category. Use category color when available.
            var pie = summary.ByCategory
                .Select((c, i) => {
                    // default fallback color if parsing fails
                    var hex = string.IsNullOrWhiteSpace(c.Color) ? "#007AFF" : c.Color;
                    int argb = 0xFF007AFF; // fallback ARGB
                    try
                    {
                        var hexDigits = hex.TrimStart('#');
                        var rgb = int.Parse(hexDigits, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        argb = unchecked((int)(0xFF000000 | (uint)rgb));
                    }
                    catch { }

                    return (ISeries)new PieSeries<double>
                    {
                        Values = new double[] { (double)c.Total },
                        Name = c.CategoryName,
                        Fill = new SKColor(argb)
                    };
                })
                .ToArray();

            PieSeries = pie;

            // Bar: if category selected, show months for that category; otherwise show monthly totals
            if (string.IsNullOrEmpty(categoryId))
            {
                var months = summary.Months.OrderBy(m => m.Month).Select(m => (double)m.Total).ToArray();
                BarSeries = new ISeries[] { new ColumnSeries<double> { Values = months } };
            }
            else
            {
                var months = summary.Months.OrderBy(m => m.Month)
                    .Select(m => (double)(m.ByCategory.FirstOrDefault(b => b.CategoryId == categoryId)?.Total ?? 0m))
                    .ToArray();
                BarSeries = new ISeries[] { new ColumnSeries<double> { Values = months } };
            }
        }
    }
}
