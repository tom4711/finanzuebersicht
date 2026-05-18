using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class DeleteRecurringTransactionUseCase(IRecurringTransactionRepository recurringTransactionRepository)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task ExecuteAsync(string recurringTransactionId, CancellationToken cancellationToken = default)
    {
        await _recurringTransactionRepository.DeleteRecurringTransactionAsync(recurringTransactionId);
    }
}