using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        public YearOverviewViewModel()
        {
            Year = DateTime.Now.Year;
            LoadCommand = new AsyncRelayCommand(LoadAsync);

            // Sample data for prototype (replace with real aggregation)
            PieSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { 350.0, 150.0, 200.0 }, Name = "Essen", Fill = new SKColor(0xFF4CAF50) },
                new PieSeries<double> { Values = new double[] { 120.0 }, Name = "Transport", Fill = new SKColor(0xFFFF9800) },
                new PieSeries<double> { Values = new double[] { 80.0 }, Name = "Freizeit", Fill = new SKColor(0xFF2196F3) }
            };

            BarSeries = new ISeries[]
            {
                new ColumnSeries<double> { Values = new double[] { 400, 320, 280, 300, 360, 420, 380, 300, 310, 290, 330, 370 } }
            };
        }

        [ObservableProperty]
        private int year;

        public ISeries[] PieSeries { get; set; }
        public ISeries[] BarSeries { get; set; }

        public IAsyncRelayCommand LoadCommand { get; }

        private Task LoadAsync()
        {
            // TODO: Load real aggregated data from IDataService
            return Task.CompletedTask;
        }
    }
}
