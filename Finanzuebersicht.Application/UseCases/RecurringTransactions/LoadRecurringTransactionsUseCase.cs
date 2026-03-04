using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class LoadRecurringTransactionsUseCase
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;

    public LoadRecurringTransactionsUseCase(IRecurringTransactionRepository recurringTransactionRepository)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task<List<RecurringTransaction>> ExecuteAsync()
    {
        var liste = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        return liste.OrderByDescending(d => d.Aktiv).ThenBy(d => d.Titel).ToList();
    }
}