using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class CashflowPage : BaseContentPage
{
    public CashflowPage(CashflowViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
