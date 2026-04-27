using Finanzuebersicht.Views;

namespace Finanzuebersicht;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(TransactionDetailPage), typeof(TransactionDetailPage));
		Routing.RegisterRoute(nameof(RecurringTransactionDetailPage), typeof(RecurringTransactionDetailPage));
		Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
		Routing.RegisterRoute(nameof(RecurringInstanceShiftPage), typeof(RecurringInstanceShiftPage));
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
		Routing.RegisterRoute(nameof(BackupListPage), typeof(BackupListPage));
	}

	private async void OnSettingsClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(SettingsPage));
	}
}
