using Finanzuebersicht.ViewModels;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Views;

public partial class TransactionsPage : BaseContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel, ILogger<TransactionsPage> logger)
    {
        InitializeComponent();

        if (viewModel == null)
        {
            logger?.LogError("TransactionsPage: injected TransactionsViewModel is null. DI may have failed.");
            try { Finanzuebersicht.Services.FileLogger.Append("TransactionsPage", "injected TransactionsViewModel is null"); } catch { }
            BindingContext = new object();
        }
        else
        {
            BindingContext = viewModel;
        }
    }
}
