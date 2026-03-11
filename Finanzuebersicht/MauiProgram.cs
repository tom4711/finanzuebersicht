using CommunityToolkit.Maui;
using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Infrastructure;
using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;
using Finanzuebersicht.Views;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Core.Services;

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

#if MACCATALYST || IOS
		// Faster tab switch animation via custom Shell renderer
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<Shell, Finanzuebersicht.Handlers.FastShellRenderer>();
		});
#endif

		// Services
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddInfrastructureServices();
		builder.Services.AddSingleton<IRecurringGenerationService, RecurringGenerationService>();
		builder.Services.AddSingleton<IReportingService, ReportingService>();
		builder.Services.AddSingleton<IDataService, DataServiceFacade>();
		builder.Services.AddSingleton<ITransactionValidationService, TransactionValidationService>();
		builder.Services.AddSingleton<IBackupService, BackupService>();
		// Import/parsers
		// register parser explicitly using DI extension to avoid ambiguous CommunityToolkit overloads
			Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IStatementParser, DkbCsvParser>(builder.Services);
		builder.Services.AddSingleton<ImportService>();
		// Categorization strategies
		Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<ICategorizationStrategy, KeywordCategorizationStrategy>(builder.Services);
		Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<ICategorizationStrategy, HistoricalCategorizationStrategy>(builder.Services);
		builder.Services.AddSingleton<CategorizationService>();

		builder.Services.AddTransient<DeleteCategoryUseCase>();
		builder.Services.AddTransient<LoadCategoriesUseCase>();
		builder.Services.AddTransient<SaveCategoryDetailUseCase>();
		builder.Services.AddTransient<LoadDashboardMonthUseCase>();
		builder.Services.AddTransient<LoadDashboardYearUseCase>();
		builder.Services.AddTransient<DeleteRecurringTransactionUseCase>();
		builder.Services.AddTransient<LoadRecurringTransactionDetailDataUseCase>();
		builder.Services.AddTransient<LoadRecurringTransactionsUseCase>();
		builder.Services.AddTransient<ToggleRecurringTransactionActiveUseCase>();
		builder.Services.AddTransient<LoadTransactionDetailDataUseCase>();
		builder.Services.AddTransient<DeleteTransactionUseCase>();
		builder.Services.AddTransient<LoadTransactionsMonthUseCase>();
		builder.Services.AddTransient<SaveRecurringTransactionDetailUseCase>();
		builder.Services.AddTransient<SaveTransactionDetailUseCase>();
		builder.Services.AddSingleton<InitializationService>();
		builder.Services.AddSingleton<ThemeService>();
		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
		builder.Services.AddSingleton<IDialogService, ShellDialogService>();

		// ViewModels
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<CategoriesViewModel>();
		builder.Services.AddTransient<CategoryDetailViewModel>();
		builder.Services.AddTransient<TransactionsViewModel>();
		builder.Services.AddTransient<TransactionDetailViewModel>();
		builder.Services.AddTransient<RecurringTransactionsViewModel>();
		builder.Services.AddTransient<RecurringTransactionDetailViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<YearOverviewViewModel>();

		// Pages
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<TransactionsPage>();
		builder.Services.AddTransient<TransactionDetailPage>();
		builder.Services.AddTransient<RecurringTransactionsPage>();
		builder.Services.AddTransient<RecurringTransactionDetailPage>();
		builder.Services.AddTransient<CategoriesPage>();
		builder.Services.AddTransient<CategoryDetailPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<YearOverviewPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		return app;
	}
}
