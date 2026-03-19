using Finanzuebersicht.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Finanzuebersicht.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register specialized data stores as singletons
        services.AddSingleton<CategoryStore>();
        services.AddSingleton<TransactionStore>();
        services.AddSingleton<RecurringStore>();

        // Register composite LocalDataService which coordinates all stores
        services.AddSingleton<LocalDataService>(sp =>
            new LocalDataService(sp.GetRequiredService<SettingsService>(), sp.GetRequiredService<Finanzuebersicht.Core.Services.IClock>(), sp.GetService<Microsoft.Extensions.Logging.ILogger<LocalDataService>>()));

        // Expose stores via their repository interfaces
        services.AddSingleton<ICategoryRepository>(sp => sp.GetRequiredService<LocalDataService>());
        services.AddSingleton<ITransactionRepository>(sp => sp.GetRequiredService<LocalDataService>());
        services.AddSingleton<IRecurringTransactionRepository>(sp => sp.GetRequiredService<LocalDataService>());

        return services;
    }
}