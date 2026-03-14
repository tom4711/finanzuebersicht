using System.Linq;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class RemoveRecurringExceptionUseCase
(
    IRecurringTransactionRepository recurringTransactionRepository
)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task ExecuteAsync(string recurringTransactionId, string exceptionId)
    {
        var list = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        var recurring = list.FirstOrDefault(r => r.Id == recurringTransactionId);
        if (recurring == null || recurring.Exceptions == null) return;

        var ex = recurring.Exceptions.FirstOrDefault(e => e.Id == exceptionId);
        if (ex == null) return;

        recurring.Exceptions.Remove(ex);
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
    }
}
