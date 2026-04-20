using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;
        private readonly ILogger<YearOverviewViewModel>? _logger;

        public YearOverviewViewModel(LoadDashboardYearUseCase loadDashboardYearUseCase, Microsoft.Extensions.Logging.ILogger<YearOverviewViewModel>? logger = null, Finanzuebersicht.Core.Services.IClock? clock = null)
        {
            _loadDashboardYearUseCase = loadDashboardYearUseCase;
            _logger = logger;
            var c = clock ?? Finanzuebersicht.Core.Services.SystemClock.Instance;
            Year = c.Now.Year;
            Categories = new ObservableCollection<CategorySummary>();
        }

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private decimal yearTotal;

        [ObservableProperty]
        private ObservableCollection<CategorySummary> categories;

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
