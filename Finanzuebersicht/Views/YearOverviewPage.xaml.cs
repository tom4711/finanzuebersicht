using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class YearOverviewPage : ContentPage
{
    private readonly YearOverviewViewModel _vm;

    public YearOverviewPage(YearOverviewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"YearOverviewPage error: {ex}");
        }
    }
}
