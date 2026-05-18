using Finanzuebersicht.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Finanzuebersicht.Presentation.DependencyInjection;

public static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationViewModels(this IServiceCollection services)
    {
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<CategoryDetailViewModel>();
        services.AddTransient<TransactionsViewModel>();
        services.AddTransient<TransactionDetailViewModel>();
        services.AddTransient<RecurringTransactionsViewModel>();
        services.AddTransient<RecurringTransactionDetailViewModel>();
        services.AddTransient<RecurringInstanceShiftViewModel>();
        services.AddTransient<AppearanceViewModel>();
        services.AddTransient<StorageViewModel>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SparZieleViewModel>();
        services.AddTransient<BackupListViewModel>();

        return services;
    }
}
