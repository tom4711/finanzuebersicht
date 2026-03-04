using Finanzuebersicht.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Finanzuebersicht.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<LocalDataService>(sp =>
            new LocalDataService(sp.GetRequiredService<SettingsService>()));

        services.AddSingleton<ICategoryRepository>(sp => sp.GetRequiredService<LocalDataService>());
        services.AddSingleton<ITransactionRepository>(sp => sp.GetRequiredService<LocalDataService>());
        services.AddSingleton<IRecurringTransactionRepository>(sp => sp.GetRequiredService<LocalDataService>());

        return services;
    }
}