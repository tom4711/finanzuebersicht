using Finanzuebersicht.ViewModels;
using Microsoft.Maui.Controls;

namespace Finanzuebersicht.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TransactionsViewModel vm)
            vm.LoadTransaktionenCommand.Execute(null);
    }
}
