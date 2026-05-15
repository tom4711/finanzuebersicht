using Finanzuebersicht.Navigation;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class TransactionDetailPage : BaseContentPage, IQueryAttributable
{
    public TransactionDetailPage(TransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is IApplyQueryAttributes vm)
            vm.ApplyQueryAttributes(query);
    }
}
