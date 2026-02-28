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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _vm.LoadCommand.ExecuteAsync(null);
    }
}
