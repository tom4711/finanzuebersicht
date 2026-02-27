using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class TransactionDetailPage : ContentPage
{
    public TransactionDetailPage(TransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TransactionDetailViewModel vm)
            vm.LoadKategorienCommand.Execute(null);
    }
}
