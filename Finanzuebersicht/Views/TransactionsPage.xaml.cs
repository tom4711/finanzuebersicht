using Finanzuebersicht.ViewModels;
using Microsoft.Maui.Controls;

namespace Finanzuebersicht.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel, Microsoft.Extensions.Logging.ILogger<TransactionsPage> logger)
    {
        InitializeComponent();

        if (viewModel == null)
        {
            logger?.LogError("TransactionsPage: injected TransactionsViewModel is null. DI may have failed.");
            // still set an empty BindingContext to avoid crashes
            BindingContext = new TransactionsViewModel(
                new DeleteTransactionUseCase(null),
                new LoadTransactionsMonthUseCase(null),
                new ShellNavigationService(),
                null,
                null,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<TransactionsViewModel>.Instance
            );
        }
        else
        {
            logger?.LogDebug("TransactionsPage: TransactionsViewModel injected successfully");
            BindingContext = viewModel;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TransactionsViewModel vm)
            vm.LoadTransaktionenCommand.Execute(null);
    }
}
