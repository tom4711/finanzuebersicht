using System.Linq;
using System.Collections.Generic;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class AddRecurringExceptionUseCase
(
    IRecurringTransactionRepository recurringTransactionRepository
)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task ExecuteAsync(string recurringTransactionId, RecurringException exception)
    {
        var list = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        var recurring = list.FirstOrDefault(r => r.Id == recurringTransactionId);
        if (recurring == null) return;

        recurring.Exceptions ??= new List<RecurringException>();
        var existing = recurring.Exceptions.FirstOrDefault(e => e.InstanceDate.Date == exception.InstanceDate.Date);
        if (existing != null)
        {
            // replace/merge existing exception for the same instance date
            existing.Type = exception.Type;
            existing.ShiftToDate = exception.ShiftToDate;
            existing.Note = exception.Note;
            existing.Id = exception.Id;
        }
        else
        {
            recurring.Exceptions.Add(exception);
        }
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
    }
}
