using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;

        public YearOverviewViewModel(LoadDashboardYearUseCase loadDashboardYearUseCase)
        {
            _loadDashboardYearUseCase = loadDashboardYearUseCase;
            Year = DateTime.Now.Year;
            Categories = new List<CategorySummary>();
        }

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private decimal yearTotal;

        [ObservableProperty]
        private List<CategorySummary> categories;

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
                        Categories = data.Kategorien;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadAsync MainThread error: {ex}");
                        YearTotal = 0;
                        Categories = new List<CategorySummary>();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAsync error: {ex}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    YearTotal = 0;
                    Categories = new List<CategorySummary>();
                });
            }
        }
    }
}
