using System;
using System.Linq;
using System.Collections.Generic;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class DueRecurringItem
{
    public RecurringTransaction Recurring { get; set; } = null!;
    public bool IsDue { get; set; }
    public string? Hint { get; set; }
}

public class GetDueRecurringWithHintsUseCase(
    IRecurringTransactionRepository recurringTransactionRepository)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task<List<DueRecurringItem>> ExecuteAsync(
        DateTime referenceDate,
        CancellationToken cancellationToken = default)
    {
        var list = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        var result = new List<DueRecurringItem>();

        foreach (var r in list.Where(x => x.Aktiv)
                               .Where(x => RecurringScheduleCalculator.IsWithinActiveRange(x, referenceDate)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dueDate = RecurringScheduleCalculator.GetNextDueDate(r, referenceDate);
            if (dueDate is null)
                continue;

            var daysUntil = (dueDate.Value.Date - referenceDate.Date).Days;
            var isDue = daysUntil <= 0;
            string? hint = daysUntil switch
            {
                0 => "Heute fällig",
                < 0 => $"Seit {-daysUntil} Tagen überfällig",
                _ when r.ReminderDaysBefore > 0 && daysUntil <= r.ReminderDaysBefore => $"Fällig in {daysUntil} Tagen",
                _ => null,
            };

            result.Add(new DueRecurringItem { Recurring = r, IsDue = isDue, Hint = hint });
        }

        return result;
    }
}

