using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class RecurringTransactionDetailPage : BaseContentPage
{
    public RecurringTransactionDetailPage(RecurringTransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
