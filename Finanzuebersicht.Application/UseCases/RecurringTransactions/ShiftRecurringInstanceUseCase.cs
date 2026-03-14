using System.Linq;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class ShiftRecurringInstanceUseCase
(
    IRecurringTransactionRepository recurringTransactionRepository
)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task ExecuteAsync(string recurringTransactionId, DateTime instanceDate, DateTime newDate, string? note = null)
    {
        var list = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        var recurring = list.FirstOrDefault(r => r.Id == recurringTransactionId);
        if (recurring == null) return;

        recurring.Exceptions ??= new List<RecurringException>();
        var ex = new RecurringException
        {
            InstanceDate = instanceDate.Date,
            Type = RecurringExceptionType.Shift,
            ShiftToDate = newDate.Date,
            Note = note
        };

        recurring.Exceptions.Add(ex);
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
    }
}
