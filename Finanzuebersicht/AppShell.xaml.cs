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
	}
}
