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

public class GetDueRecurringWithHintsUseCase
(
    IRecurringTransactionRepository recurringTransactionRepository
)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task<List<DueRecurringItem>> ExecuteAsync(DateTime referenceDate)
    {
        var list = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        var result = new List<DueRecurringItem>();
        foreach (var r in list.Where(x => x.Aktiv))
        {
            // approximate next occurrence: if LetzteAusfuehrung unset use Startdatum
            var start = r.LetzteAusfuehrung ?? r.Startdatum;
            var approxNext = start;
            // naive stepping: advance until >= referenceDate
            while (approxNext.Date < referenceDate.Date)
            {
                approxNext = approxNext.AddMonths(1);
            }

            var daysUntil = (approxNext.Date - referenceDate.Date).Days;
            var isDue = daysUntil <= 0;
            var hint = r.ReminderDaysBefore > 0 && daysUntil <= r.ReminderDaysBefore ? $"Fällig in {daysUntil} Tagen" : null;
            result.Add(new DueRecurringItem { Recurring = r, IsDue = isDue, Hint = hint });
        }

        return result;
    }
}
