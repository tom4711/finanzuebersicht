using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class ToggleRecurringTransactionActiveUseCase
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;

    public ToggleRecurringTransactionActiveUseCase(IRecurringTransactionRepository recurringTransactionRepository)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task ExecuteAsync(RecurringTransaction recurringTransaction)
    {
        recurringTransaction.Aktiv = !recurringTransaction.Aktiv;
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurringTransaction);
    }
}