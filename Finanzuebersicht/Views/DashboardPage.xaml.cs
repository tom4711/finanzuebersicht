using Finanzuebersicht.Charts;
using Finanzuebersicht.ViewModels;
using System.ComponentModel;

namespace Finanzuebersicht.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;
    private readonly DonutChartDrawable _monthDonut = new();
    private readonly BarChartDrawable _yearBar = new();
    private readonly DonutChartDrawable _yearDonut = new();

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;

        MonthDonutChart.Drawable = _monthDonut;
        YearBarChart.Drawable = _yearBar;
        YearDonutChart.Drawable = _yearDonut;

        // Initialize drawables with current VM data so charts render immediately
        _monthDonut.Items = _vm.KategorieAusgaben;
        _yearBar.Months = _vm.JahrMonate;
        _yearBar.CurrentMonth = _vm.IsYearView ? 0 : _vm.AktuellerMonat.Month;
        _yearDonut.Items = _vm.JahrKategorien;

        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            vm.LoadDashboardCommand.Execute(null);
        }

        // Ensure charts are invalidated after loading so drawables render current data
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MonthDonutChart.Invalidate();
            YearDonutChart.Invalidate();
            YearBarChart.Invalidate();
        });

        // subscribe to app-wide data change notifications
        App.DataChanged += OnAppDataChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        App.DataChanged -= OnAppDataChanged;
    }

    private void OnAppDataChanged()
    {
        if (BindingContext is DashboardViewModel vm)
            _ = vm.LoadDashboardCommand.ExecuteAsync(null);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DashboardViewModel.KategorieAusgaben):
                _monthDonut.Items = _vm.KategorieAusgaben;
                MainThread.BeginInvokeOnMainThread(() => MonthDonutChart.Invalidate());
                break;
            case nameof(DashboardViewModel.JahrMonate):
                _yearBar.Months = _vm.JahrMonate;
                _yearBar.CurrentMonth = _vm.IsYearView ? 0 : Finanzuebersicht.Core.Services.SystemClock.Instance.Today.Month;
                MainThread.BeginInvokeOnMainThread(() => YearBarChart.Invalidate());
                break;
            case nameof(DashboardViewModel.JahrKategorien):
                _yearDonut.Items = _vm.JahrKategorien;
                MainThread.BeginInvokeOnMainThread(() => YearDonutChart.Invalidate());
                break;
        }
    }
}
