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
            // determine the next candidate instance using same rules as generator
            DateTime candidate;
            if (r.LetzteAusfuehrung.HasValue)
            {
                candidate = GetNextInstance(r, r.LetzteAusfuehrung.Value, r.IntervalFactor);
            }
            else
            {
                candidate = r.Startdatum.Date;
            }

            while (candidate.Date < referenceDate.Date)
            {
                candidate = GetNextInstance(r, candidate, r.IntervalFactor);
            }

            var daysUntil = (candidate.Date - referenceDate.Date).Days;
            var isDue = daysUntil <= 0;
            string? hint = null;
            if (daysUntil == 0)
                hint = "Heute fällig";
            else if (daysUntil < 0)
                hint = $"Seit {-daysUntil} Tagen überfällig";
            else if (r.ReminderDaysBefore > 0 && daysUntil <= r.ReminderDaysBefore)
                hint = $"Fällig in {daysUntil} Tagen";

            result.Add(new DueRecurringItem { Recurring = r, IsDue = isDue, Hint = hint });
        }

        return result;

    }

    private static DateTime GetNextInstance(RecurringTransaction recurring, DateTime fromDate, int intervalFactor)
    {
        var factor = Math.Max(1, intervalFactor);
        return recurring.Interval switch
        {
            RecurrenceInterval.Weekly => fromDate.Date.AddDays(7L * factor),
            RecurrenceInterval.Monthly => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
            RecurrenceInterval.Quarterly => AddMonthsPreserveDay(fromDate.Date, 3 * factor),
            RecurrenceInterval.Yearly => AddMonthsPreserveDay(fromDate.Date, 12 * factor),
            RecurrenceInterval.Daily => fromDate.Date.AddDays(1 * factor),
            _ => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
        };
    }

    private static DateTime AddMonthsPreserveDay(DateTime date, int months)
    {
        var target = date.AddMonths(months);
        var day = date.Day;
        var daysInTarget = DateTime.DaysInMonth(target.Year, target.Month);
        if (day > daysInTarget)
            day = daysInTarget;
        return new DateTime(target.Year, target.Month, day);
    }

}
