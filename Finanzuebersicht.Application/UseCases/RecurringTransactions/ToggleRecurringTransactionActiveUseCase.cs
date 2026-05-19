using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class ToggleRecurringTransactionActiveUseCase(IRecurringTransactionRepository recurringTransactionRepository)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task ExecuteAsync(RecurringTransaction recurringTransaction, CancellationToken cancellationToken = default)
    {
        recurringTransaction.Aktiv = !recurringTransaction.Aktiv;
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurringTransaction);
    }
}