using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoriesViewModel vm)
            vm.LoadKategorienCommand.Execute(null);
    }
}
