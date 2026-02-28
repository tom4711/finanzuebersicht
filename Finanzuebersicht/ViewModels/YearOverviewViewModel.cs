using System;
using System.Linq;
using System.Threading.Tasks;
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

            // keep empty series until data is loaded
            PieSeries = Array.Empty<ISeries>();
            BarSeries = Array.Empty<ISeries>();
        }

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private ISeries[] pieSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] barSeries = Array.Empty<ISeries>();

        public IAsyncRelayCommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            var summary = await _dataService.GetYearSummaryAsync(Year);
            // Pie: by category
            var colors = new uint[] { 0xFF4CAF50, 0xFFFF9800, 0xFF2196F3, 0xFFE91E63, 0xFF9C27B0, 0xFFFFC107 };
            var pie = summary.ByCategory
                .Select((c, i) => (ISeries)new PieSeries<double>
                {
                    Values = new double[] { (double)c.Total },
                    Name = c.CategoryName,
                    Fill = new SKColor((int)colors[i % colors.Length])
                })
                .ToArray();

            PieSeries = pie;

            // Bar: monthly totals
            var months = summary.Months.OrderBy(m => m.Month).Select(m => (double)m.Total).ToArray();
            BarSeries = new ISeries[] { new ColumnSeries<double> { Values = months } };
        }
    }
}
