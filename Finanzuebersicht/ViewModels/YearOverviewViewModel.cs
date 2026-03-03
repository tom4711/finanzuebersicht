using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Services;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        private readonly IReportingService _reportingService;

        public YearOverviewViewModel(IReportingService reportingService)
        {
            _reportingService = reportingService;
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
                var summary = await _reportingService.GetYearSummaryAsync(Year);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (summary != null)
                        {
                            YearTotal = summary.Total;
                            
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
