using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

/// <summary>
/// Pure domain component for calculating recurring transaction schedules.
/// Single source of truth for: next-due calculation, exception application,
/// and active-range checks. Used by both <see cref="RecurringGenerationService"/>
/// and <c>GetDueRecurringWithHintsUseCase</c>.
/// </summary>
public static class RecurringScheduleCalculator
{
    /// <summary>
    /// Returns the next instance date after <paramref name="fromDate"/> based on
    /// the recurring transaction's interval and intervalFactor.
    /// </summary>
    public static DateTime GetNextInstance(RecurringTransaction recurring, DateTime fromDate)
    {
        var factor = Math.Max(1, recurring.IntervalFactor);
        return recurring.Interval switch
        {
            RecurrenceInterval.Daily => fromDate.Date.AddDays(1 * factor),
            RecurrenceInterval.Weekly => fromDate.Date.AddDays(7L * factor),
            RecurrenceInterval.Monthly => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
            RecurrenceInterval.Quarterly => AddMonthsPreserveDay(fromDate.Date, 3 * factor),
            RecurrenceInterval.Yearly => AddMonthsPreserveDay(fromDate.Date, 12 * factor),
            _ => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
        };
    }

    /// <summary>
    /// Applies any defined Skip or Shift exceptions to <paramref name="candidate"/>.
    /// Returns the effective date after applying exceptions.
    /// For a Skip, advances to the next instance. For a Shift with a ShiftToDate, returns that date.
    /// </summary>
    public static DateTime ApplyExceptions(RecurringTransaction recurring, DateTime candidate)
    {
        var adjusted = candidate.Date;

        if (recurring.Exceptions is null || recurring.Exceptions.Count == 0)
            return adjusted;

        var exception = recurring.Exceptions.FirstOrDefault(e => e.InstanceDate.Date == adjusted);
        if (exception is null)
            return adjusted;

        return exception.Type switch
        {
            RecurringExceptionType.Skip => GetNextInstance(recurring, adjusted),
            RecurringExceptionType.Shift when exception.ShiftToDate.HasValue => exception.ShiftToDate.Value.Date,
            _ => adjusted,
        };
    }

    /// <summary>
    /// Returns true if <paramref name="referenceDate"/> falls within the active range of
    /// the recurring transaction (accounting for ReminderDaysBefore and Enddatum).
    /// </summary>
    public static bool IsWithinActiveRange(RecurringTransaction recurring, DateTime referenceDate)
    {
        var reference = referenceDate.Date;
        var start = recurring.Startdatum.Date;

        if (reference < start.AddDays(-recurring.ReminderDaysBefore))
            return false;

        if (recurring.Enddatum.HasValue && reference > recurring.Enddatum.Value.Date)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates the next due date for a recurring transaction relative to
    /// <paramref name="referenceDate"/>, with exceptions applied.
    /// Returns null if the transaction is not active or outside its date range.
    /// </summary>
    public static DateTime? GetNextDueDate(RecurringTransaction recurring, DateTime referenceDate)
    {
        if (!recurring.Aktiv || !IsWithinActiveRange(recurring, referenceDate))
            return null;

        var candidate = recurring.LetzteAusfuehrung.HasValue
            ? GetNextInstance(recurring, recurring.LetzteAusfuehrung.Value)
            : recurring.Startdatum.Date;

        while (candidate.Date < referenceDate.Date)
            candidate = GetNextInstance(recurring, candidate);

        return ApplyExceptions(recurring, candidate);
    }

    /// <summary>
    /// Adds months to a date while clamping the day to the last day of the target month.
    /// For example, Jan 31 + 1 month = Feb 28 (or 29 in a leap year).
    /// </summary>
    public static DateTime AddMonthsPreserveDay(DateTime date, int months)
    {
        var target = date.AddMonths(months);
        var day = date.Day;
        var daysInTarget = DateTime.DaysInMonth(target.Year, target.Month);
        if (day > daysInTarget)
            day = daysInTarget;
        return new DateTime(target.Year, target.Month, day);
    }
}
