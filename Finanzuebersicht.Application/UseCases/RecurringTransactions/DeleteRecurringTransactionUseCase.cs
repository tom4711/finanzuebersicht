using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class DeleteRecurringTransactionUseCase
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;

    public DeleteRecurringTransactionUseCase(IRecurringTransactionRepository recurringTransactionRepository)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task ExecuteAsync(string recurringTransactionId)
    {
        await _recurringTransactionRepository.DeleteRecurringTransactionAsync(recurringTransactionId);
    }
}