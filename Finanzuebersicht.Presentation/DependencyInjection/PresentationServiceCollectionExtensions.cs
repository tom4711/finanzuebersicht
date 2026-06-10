using Finanzuebersicht.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Finanzuebersicht.Presentation.DependencyInjection;

public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all presentation ViewModels.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="appAssembly">
    /// The entry assembly of the host application, used by <see cref="AboutViewModel"/>
    /// to display the correct version string. When <c>null</c>, falls back to
    /// <see cref="Assembly.GetExecutingAssembly"/> (the Presentation assembly).
    /// Pass <c>Assembly.GetExecutingAssembly()</c> from <c>MauiProgram</c> so the
    /// MAUI app assembly version is shown on the About page.
    /// </param>
    public static IServiceCollection AddPresentationViewModels(this IServiceCollection services, Assembly? appAssembly = null)
    {
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<CategoryDetailViewModel>();
        services.AddTransient<AccountDetailViewModel>();
        services.AddTransient<TransferDetailViewModel>();
        services.AddTransient<TransactionsViewModel>();
        services.AddTransient<TransactionDetailViewModel>();
        services.AddTransient<RecurringTransactionsViewModel>();
        services.AddTransient<RecurringTransactionDetailViewModel>();
        services.AddTransient<RecurringInstanceShiftViewModel>();
        services.AddTransient<AppearanceViewModel>();
        services.AddTransient<StorageViewModel>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<AboutViewModel>(sp =>
            new AboutViewModel(appAssembly, sp.GetService<Microsoft.Extensions.Logging.ILogger<AboutViewModel>>()));
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SparZieleViewModel>();
        services.AddTransient<BackupListViewModel>();
        services.AddTransient<ImportPreviewViewModel>();
        services.AddTransient<CashflowViewModel>();

        return services;
    }
}
