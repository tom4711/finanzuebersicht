using Finanzuebersicht.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel, ILogger<TransactionsPage> logger)
    {
        InitializeComponent();

        if (viewModel == null)
        {
            logger?.LogError("TransactionsPage: injected TransactionsViewModel is null. DI may have failed.");
            try { Finanzuebersicht.Core.Services.FileLogger.Append("TransactionsPage", "injected TransactionsViewModel is null"); } catch { }
            // avoid creating types from other layers here — leave BindingContext empty to prevent further NREs
            BindingContext = new object();
        }
        else
        {
            logger?.LogDebug("TransactionsPage: TransactionsViewModel injected successfully");
            try { Finanzuebersicht.Core.Services.FileLogger.Append("TransactionsPage", "TransactionsViewModel injected successfully"); } catch { }
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
