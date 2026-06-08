using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadCashflowOutlookUseCase(
    ITransactionRepository transactionRepository,
    IRecurringTransactionRepository recurringTransactionRepository,
    Finanzuebersicht.Core.Services.IClock? clock = null)
{
    private readonly Finanzuebersicht.Core.Services.IClock _clock = clock ?? SystemClock.Instance;
    private const decimal NotableDayThreshold = 100m;

    public async Task<CashflowOutlookData> ExecuteAsync(
        int horizonDays = 30,
        string? accountId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var from = _clock.Today.Date;
        var to = from.AddDays(Math.Max(1, horizonDays));
        var entries = new List<CashflowEntry>();

        var transactions = await transactionRepository.GetTransactionsAsync(from, to);
        if (!string.IsNullOrWhiteSpace(accountId))
            transactions = transactions.Where(t => t.AccountId == accountId).ToList();

        foreach (var transaction in transactions.Where(t => !t.IsTransfer))
        {
            entries.Add(new CashflowEntry
            {
                Date = transaction.Datum.Date,
                Title = transaction.Titel,
                Amount = Math.Abs(transaction.Betrag),
                Typ = transaction.Typ,
                IsProjected = false
            });
        }

        var recurringItems = await recurringTransactionRepository.GetRecurringTransactionsAsync();
        var bookedRecurringKeys = transactions
            .Where(t => !string.IsNullOrWhiteSpace(t.DauerauftragId))
            .Select(t => $"{t.DauerauftragId}:{t.Datum.Date:yyyy-MM-dd}")
            .ToHashSet();

        foreach (var recurring in recurringItems.Where(r => r.Aktiv))
        {
            if (!string.IsNullOrWhiteSpace(accountId) && recurring.AccountId != accountId)
                continue;

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (!OccursOnDate(recurring, day))
                    continue;

                var key = $"{recurring.Id}:{day:yyyy-MM-dd}";
                if (bookedRecurringKeys.Contains(key))
                    continue;

                entries.Add(new CashflowEntry
                {
                    Date = day,
                    Title = recurring.Titel,
                    Amount = Math.Abs(recurring.Betrag),
                    Typ = recurring.Typ,
                    IsProjected = true
                });
            }
        }

        var days = entries
            .GroupBy(e => e.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var income = g.Where(e => e.Typ == TransactionType.Einnahme).Sum(e => e.Amount);
                var expenses = g.Where(e => e.Typ == TransactionType.Ausgabe).Sum(e => e.Amount);
                var net = income - expenses;
                return new CashflowDayGroup
                {
                    Date = g.Key,
                    Entries = g.OrderBy(e => e.Typ).ThenBy(e => e.Title).ToList(),
                    NetAmount = net,
                    IsNotable = Math.Abs(net) >= NotableDayThreshold
                };
            })
            .ToList();

        return new CashflowOutlookData
        {
            From = from,
            To = to,
            Days = days,
            ProjectedIncome = entries.Where(e => e.Typ == TransactionType.Einnahme).Sum(e => e.Amount),
            ProjectedExpenses = entries.Where(e => e.Typ == TransactionType.Ausgabe).Sum(e => e.Amount)
        };
    }

    private static bool OccursOnDate(RecurringTransaction recurring, DateTime date)
    {
        var instance = RecurringScheduleCalculator.GetNextDueInstance(recurring, date);
        return instance.HasValue && instance.Value.EffectiveDate.Date == date.Date;
    }
}
