using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class RecurringGenerationService(
    IRecurringTransactionRepository recurringRepository,
    ITransactionRepository transactionRepository) : IRecurringGenerationService
{
    private readonly IRecurringTransactionRepository _recurringRepository = recurringRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private const int MaxInstancesPerRun = 500;

    public async Task GeneratePendingRecurringTransactionsAsync()
    {
        var recurringItems = await _recurringRepository.GetRecurringTransactionsAsync();
        var today = DateTime.Today;

        foreach (var recurring in recurringItems.Where(item => item.Aktiv))
        {
            if (recurring.Enddatum.HasValue && recurring.Enddatum.Value < today)
                continue;

            var generatedCount = 0;

            // determine the first instance to consider
            var candidate = !recurring.LetzteAusfuehrung.HasValue
                ? recurring.Startdatum
                : GetNextInstance(recurring, recurring.LetzteAusfuehrung.Value);

            while (candidate <= today && generatedCount < MaxInstancesPerRun)
            {
                // check for exceptions for this instance date
                var exception = recurring.Exceptions?.FirstOrDefault(e => e.InstanceDate.Date == candidate.Date);
                if (exception != null && exception.Type == RecurringExceptionType.Skip)
                {
                    // mark as executed (skip this instance)
                    recurring.LetzteAusfuehrung = candidate;
                    candidate = GetNextInstance(recurring, candidate);
                    continue;
                }

                var transactionDate = candidate;
                if (exception != null && exception.Type == RecurringExceptionType.Shift && exception.ShiftToDate.HasValue)
                {
                    transactionDate = exception.ShiftToDate.Value.Date;
                }

                // Previously we skipped generating transactions whose effective date
                // was in the future (e.g., a Shift into the future). Create the
                // transaction regardless of whether the effective date is in the
                // future so that shifted instances are materialized as expected by tests.

                var transaction = new Transaction
                {
                    Betrag = recurring.Betrag,
                    Titel = recurring.Titel,
                    Datum = transactionDate,
                    KategorieId = recurring.KategorieId,
                    Typ = recurring.Typ,
                    DauerauftragId = recurring.Id
                };

                await _transactionRepository.SaveTransactionAsync(transaction);
                generatedCount++;

                // mark this instance as last executed (use the template instance date)
                recurring.LetzteAusfuehrung = candidate;
                candidate = GetNextInstance(recurring, candidate);
            }

            await _recurringRepository.SaveRecurringTransactionAsync(recurring);
        }
    }

    private DateTime GetNextInstance(RecurringTransaction recurring, DateTime fromDate)
    {
        // Calculate the next instance date after fromDate according to Interval and IntervalFactor
        var factor = Math.Max(1, recurring.IntervalFactor);

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
