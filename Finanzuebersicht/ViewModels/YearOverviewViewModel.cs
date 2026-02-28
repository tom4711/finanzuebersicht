using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Finanzuebersicht.ViewModels
{
    public partial class YearOverviewViewModel : ObservableObject
    {
        public YearOverviewViewModel()
        {
            Year = DateTime.Now.Year;
            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        [ObservableProperty]
        private int year;

        public IAsyncRelayCommand LoadCommand { get; }

        private Task LoadAsync()
        {
            // TODO: implement aggregation and chart data preparation
            return Task.CompletedTask;
        }
    }
}
