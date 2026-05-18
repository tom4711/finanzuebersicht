namespace Finanzuebersicht.Services;

/// <summary>
/// Broad facade interface aggregating all domain repositories and services.
/// <para>
/// This interface exists for legacy compatibility (BackupService, LocalDataService).
/// All new code should inject the specific repository interfaces directly
/// (<see cref="ICategoryRepository"/>, <see cref="ITransactionRepository"/>,
/// <see cref="IRecurringTransactionRepository"/>, <see cref="IBudgetRepository"/>,
/// <see cref="ISparZielRepository"/>).
/// </para>
/// </summary>
[Obsolete("Inject specific repository interfaces instead. IDataService will be removed in a future version.")]
public interface IDataService :
    ICategoryRepository,
    ITransactionRepository,
    IRecurringTransactionRepository,
    IRecurringGenerationService,
    IReportingService,
    IBudgetRepository,
    ISparZielRepository
{
}
