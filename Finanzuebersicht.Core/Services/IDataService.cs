namespace Finanzuebersicht.Services;

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
