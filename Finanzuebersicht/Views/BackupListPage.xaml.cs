using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class BackupListPage : BaseContentPage
{
    public BackupListPage(BackupListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
