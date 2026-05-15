using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class RecurringTransactionsPage : BaseContentPage
{
    public RecurringTransactionsPage(RecurringTransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
