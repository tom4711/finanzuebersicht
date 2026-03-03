namespace Finanzuebersicht.Services;

public interface IRecurringGenerationService
{
    Task GeneratePendingRecurringTransactionsAsync();
}
