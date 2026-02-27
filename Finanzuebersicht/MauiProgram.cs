using CommunityToolkit.Maui;
using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;
using Finanzuebersicht.Views;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<IDataService, CloudKitDataService>();
		builder.Services.AddSingleton<InitializationService>();

		// ViewModels
		builder.Services.AddTransient<CategoriesViewModel>();
		builder.Services.AddTransient<CategoryDetailViewModel>();

		// Pages
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<TransactionsPage>();
		builder.Services.AddTransient<TransactionDetailPage>();
		builder.Services.AddTransient<RecurringTransactionsPage>();
		builder.Services.AddTransient<RecurringTransactionDetailPage>();
		builder.Services.AddTransient<CategoriesPage>();
		builder.Services.AddTransient<CategoryDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
