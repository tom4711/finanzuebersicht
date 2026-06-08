using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class TransferDetailPage : BaseContentPage
{
    public TransferDetailPage(TransferDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
