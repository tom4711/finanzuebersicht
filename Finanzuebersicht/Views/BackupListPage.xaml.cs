using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class BackupListPage : ContentPage
{
    public BackupListPage(BackupListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BackupListViewModel vm)
            vm.LoadBackupsCommand.Execute(null);
    }
}
