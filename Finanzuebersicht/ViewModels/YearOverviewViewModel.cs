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
using LiveChartsCore.SkiaSharpView.Painting;
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
        private decimal yearTotal;

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
            try
            {
                var summary = await _dataService.GetYearSummaryAsync(Year);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (summary != null)
                    {
                        YearTotal = summary.Total;
                        
                        // Calculate percentages for each category
                        if (summary.ByCategory != null && summary.Total > 0)
                        {
                            foreach (var cat in summary.ByCategory)
                            {
                                cat.PercentageAmount = (cat.Total / summary.Total) * 100;
                            }
                        }
                        
                        Categories = summary.ByCategory ?? new List<CategorySummary>();
                    }
                    else
                    {
                        YearTotal = 0;
                        Categories = new List<CategorySummary>();
                    }
                });
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    YearTotal = 0;
                    Categories = new List<CategorySummary>();
                });
            }
        }

        private void SelectCategory(string? categoryId)
        {
            SelectedCategoryId = categoryId;
            var summaryTask = _dataService.GetYearSummaryAsync(Year);
            summaryTask.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result != null)
                    UpdateSeries(t.Result, categoryId);
            });
        }

        private void UpdateSeries(YearSummary summary, string? categoryId)
        {
            if (summary?.ByCategory == null || summary.ByCategory.Count == 0)
            {
                PieSeries = Array.Empty<ISeries>();
                BarSeries = Array.Empty<ISeries>();
                return;
            }

            // Pie: always show by category. Use category color when available.
            var pie = summary.ByCategory
                .Select((c, i) => {
                    var hex = string.IsNullOrWhiteSpace(c.Color) ? "#007AFF" : c.Color;
                    uint argb = 0xFF007AFF;
                    try
                    {
                        var hexDigits = hex.TrimStart('#');
                        argb = 0xFF000000 | uint.Parse(hexDigits, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    catch { }

                    return (ISeries)new PieSeries<double>
                    {
                        Values = new double[] { (double)c.Total },
                        Name = c.CategoryName,
                        Fill = new SolidColorPaint((uint)argb)
                    };
                })
                .ToArray();

            PieSeries = pie;

            // Bar: if category selected, show months for that category; otherwise show monthly totals
            if (string.IsNullOrEmpty(categoryId))
            {
                var months = summary.Months?.OrderBy(m => m.Month).Select(m => (double)m.Total).ToArray() ?? Array.Empty<double>();
                BarSeries = months.Length > 0 ? new ISeries[] { new ColumnSeries<double> { Values = months } } : Array.Empty<ISeries>();
            }
            else
            {
                var months = summary.Months?.OrderBy(m => m.Month)
                    .Select(m => (double)(m.ByCategory?.FirstOrDefault(b => b.CategoryId == categoryId)?.Total ?? 0m))
                    .ToArray() ?? Array.Empty<double>();
                BarSeries = months.Length > 0 ? new ISeries[] { new ColumnSeries<double> { Values = months } } : Array.Empty<ISeries>();
            }
        }
    }
}
