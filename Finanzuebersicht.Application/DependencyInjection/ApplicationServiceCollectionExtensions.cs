using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Application.UseCases.SparZiele;
using Finanzuebersicht.Application.UseCases.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Finanzuebersicht.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddTransient<DeleteCategoryUseCase>();
        services.AddTransient<LoadCategoriesUseCase>();
        services.AddTransient<SaveCategoryDetailUseCase>();
        services.AddTransient<SaveCategoryBudgetUseCase>();

        services.AddTransient<LoadDashboardMonthUseCase>();
        services.AddTransient<LoadDashboardYearUseCase>();
        services.AddTransient<LoadForecastUseCase>();

        services.AddTransient<DeleteRecurringTransactionUseCase>();
        services.AddTransient<LoadRecurringTransactionDetailDataUseCase>();
        services.AddTransient<LoadRecurringTransactionsUseCase>();
        services.AddTransient<ToggleRecurringTransactionActiveUseCase>();
        services.AddTransient<AddRecurringExceptionUseCase>();
        services.AddTransient<RemoveRecurringExceptionUseCase>();
        services.AddTransient<ShiftRecurringInstanceUseCase>();
        services.AddTransient<GetDueRecurringWithHintsUseCase>();

        services.AddTransient<LoadTransactionDetailDataUseCase>();
        services.AddTransient<DeleteTransactionUseCase>();
        services.AddTransient<LoadTransactionsMonthUseCase>();
        services.AddTransient<SearchTransactionsUseCase>();
        services.AddTransient<SaveRecurringTransactionDetailUseCase>();
        services.AddTransient<SaveTransactionDetailUseCase>();

        services.AddTransient<LoadSparZieleUseCase>();
        services.AddTransient<SaveSparZielUseCase>();
        services.AddTransient<DeleteSparZielUseCase>();

        services.AddSingleton<InitializationService>();

        return services;
    }
}
