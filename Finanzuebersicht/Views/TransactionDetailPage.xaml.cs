using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class TransactionDetailPage : BaseContentPage
{
    public TransactionDetailPage(TransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
