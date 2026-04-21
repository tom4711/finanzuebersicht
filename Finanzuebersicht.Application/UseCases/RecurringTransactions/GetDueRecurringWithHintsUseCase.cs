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
        foreach (var r in list.Where(x => x.Aktiv).Where(x => IsWithinActiveRange(x, referenceDate)))
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

            // apply any defined exceptions (Skip/Shift) to the calculated candidate
            candidate = ApplyExceptionsIfAny(r, candidate, r.IntervalFactor);

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

    private static bool IsWithinActiveRange(RecurringTransaction recurring, DateTime referenceDate)
    {
        var reference = referenceDate.Date;
        var start = recurring.Startdatum.Date;

        if (reference < start.AddDays(-recurring.ReminderDaysBefore))
        {
            return false;
        }

        // Enddatum is optional – access via reflection to avoid hard dependency
        var endProperty = recurring.GetType().GetProperty("Enddatum");
        if (endProperty != null)
        {
            var value = endProperty.GetValue(recurring);
            if (value is DateTime endDate && reference > endDate.Date)
            {
                return false;
            }
        }

        return true;
    }

    private static DateTime ApplyExceptionsIfAny(RecurringTransaction recurring, DateTime candidate, int intervalFactor)
    {
        var exceptionsProperty = recurring.GetType().GetProperty("Exceptions");
        if (exceptionsProperty == null)
        {
            return candidate;
        }

        var exceptionsValue = exceptionsProperty.GetValue(recurring) as System.Collections.IEnumerable;
        if (exceptionsValue == null)
        {
            return candidate;
        }

        var adjustedCandidate = candidate.Date;

        foreach (var exception in exceptionsValue)
        {
            if (exception == null)
            {
                continue;
            }

            var exceptionType = exception.GetType();

            // try to get the exception date (commonly "Date" or "Datum")
            var dateProperty =
                exceptionType.GetProperty("Date") ??
                exceptionType.GetProperty("Datum");

            if (dateProperty == null)
            {
                continue;
            }

            var dateValue = dateProperty.GetValue(exception);
            if (dateValue is not DateTime exceptionDate || exceptionDate.Date != adjustedCandidate)
            {
                continue;
            }

            // try to get the exception type (commonly "Type", "Typ" or "Art")
            var typeProperty =
                exceptionType.GetProperty("Type") ??
                exceptionType.GetProperty("Typ") ??
                exceptionType.GetProperty("Art");

            var typeName = typeProperty?.GetValue(exception)?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(typeName))
            {
                continue;
            }

            var normalizedType = typeName.Trim();

            // Skip: move to the next instance
            if (string.Equals(normalizedType, "Skip", StringComparison.OrdinalIgnoreCase))
            {
                adjustedCandidate = GetNextInstance(recurring, adjustedCandidate, intervalFactor).Date;
                continue;
            }

            // Shift: move by configured number of days if available
            if (string.Equals(normalizedType, "Shift", StringComparison.OrdinalIgnoreCase))
            {
                var shiftDaysProperty =
                    exceptionType.GetProperty("ShiftDays") ??
                    exceptionType.GetProperty("VerschiebungTage");

                var shiftValue = shiftDaysProperty?.GetValue(exception);
                if (shiftValue is int shiftDays && shiftDays != 0)
                {
                    adjustedCandidate = adjustedCandidate.AddDays(shiftDays);
                }
            }
        }

        return adjustedCandidate;
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
