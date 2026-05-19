namespace Finanzuebersicht.Core.Services;

public interface IRecurringGenerationService
{
    Task GeneratePendingRecurringTransactionsAsync(CancellationToken cancellationToken = default);
}
