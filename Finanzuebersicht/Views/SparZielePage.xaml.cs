using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class SparZielePage : ContentPage
{
    public SparZielePage(SparZieleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SparZieleViewModel vm)
            vm.LoadSparZieleCommand.Execute(null);
    }
}
