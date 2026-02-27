using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class RecurringTransactionDetailPage : ContentPage
{
    public RecurringTransactionDetailPage(RecurringTransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RecurringTransactionDetailViewModel vm)
            vm.LoadKategorienCommand.Execute(null);
    }
}
