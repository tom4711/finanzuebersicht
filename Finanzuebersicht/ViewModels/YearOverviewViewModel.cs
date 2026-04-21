using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;
        private readonly ILogger<YearOverviewViewModel>? _logger;

        public YearOverviewViewModel(LoadDashboardYearUseCase loadDashboardYearUseCase, Microsoft.Extensions.Logging.ILogger<YearOverviewViewModel>? logger = null, IClock? clock = null)
        {
            _loadDashboardYearUseCase = loadDashboardYearUseCase;
            _logger = logger;
            var c = clock ?? SystemClock.Instance;
            Year = c.Now.Year;
            Categories = new ObservableCollection<CategorySummary>();
        }

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private decimal yearTotal;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasData))]
        private ObservableCollection<CategorySummary> categories;

        public bool HasData => Categories.Count > 0;

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                var data = await _loadDashboardYearUseCase.ExecuteAsync(Year);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        YearTotal = data.GesamtAusgaben;
                        Categories = new ObservableCollection<CategorySummary>(data.Kategorien);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "LoadAsync MainThread error");
                        YearTotal = 0;
                        Categories = new ObservableCollection<CategorySummary>();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "LoadAsync error");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    YearTotal = 0;
                    Categories = new ObservableCollection<CategorySummary>();
                });
            }
        }
    }
}
