using System;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadDashboardMonthUseCase(
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IRecurringTransactionRepository recurringTransactionRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;

    public async Task<DashboardMonthData> ExecuteAsync(DateTime aktuellerMonat, DateTime today)
    {
        var kategorien = await _categoryRepository.GetCategoriesAsync();

        var von = aktuellerMonat;
        var bis = aktuellerMonat.AddMonths(1).AddDays(-1);
        var transaktionen = await _transactionRepository.GetTransactionsAsync(von, bis);

        var istPrognose = aktuellerMonat > new DateTime(today.Year, today.Month, 1);
        if (istPrognose)
        {
            var dauerauftraege = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
            foreach (var da in dauerauftraege.Where(d => d.Aktiv))
            {
                if (da.Startdatum <= bis && (!da.Enddatum.HasValue || da.Enddatum.Value >= von))
                {
                    // Only add a forecast transaction for this month if the recurrence actually occurs in the month
                    if (!transaktionen.Any(t => t.DauerauftragId == da.Id) && OccursInRange(da, von, bis))
                    {
                        transaktionen.Add(new Transaction
                        {
                            Betrag = da.Betrag,
                            Titel = da.Titel,
                            KategorieId = da.KategorieId,
                            Typ = da.Typ,
                            Datum = von,
                            DauerauftragId = da.Id
                        });
                    }
                }
            }
        }

        var gesamtEinnahmen = transaktionen
            .Where(t => t.Typ == TransactionType.Einnahme)
            .Sum(t => Math.Abs(t.Betrag));

        var gesamtAusgaben = transaktionen
            .Where(t => t.Typ == TransactionType.Ausgabe)
            .Sum(t => Math.Abs(t.Betrag));

        // Fallback-Kategorie für nicht zugeordnete Transaktionen
        var unkategorisiert = new Category { Id = string.Empty, Name = "Unkategorisiert", Color = "#8E8E93", Icon = "📁" };

        var kategorieAusgaben = transaktionen
            .Where(t => t.Typ == TransactionType.Ausgabe)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = kategorien.FirstOrDefault(k => k.Id == g.Key) ?? unkategorisiert, Total = g.Sum(t => Math.Abs(t.Betrag)) })
            .Select(x => new CategorySummary
            {
                CategoryId = x.Key,
                CategoryName = x.Cat!.Name,
                Total = (decimal)x.Total,
                Color = x.Cat.Color,
                Icon = x.Cat.Icon
            })
            .OrderByDescending(k => k.Total)
            .ToList();

        var kategorieEinnahmen = transaktionen
            .Where(t => t.Typ == TransactionType.Einnahme)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = kategorien.FirstOrDefault(k => k.Id == g.Key) ?? unkategorisiert, Total = g.Sum(t => Math.Abs(t.Betrag)) })
            .Select(x => new CategorySummary
            {
                CategoryId = x.Key,
                CategoryName = x.Cat!.Name,
                Total = (decimal)x.Total,
                Color = x.Cat.Color,
                Icon = x.Cat.Icon
            })
            .OrderByDescending(k => k.Total)
            .ToList();

        return new DashboardMonthData
        {
            IstPrognose = istPrognose,
            GesamtEinnahmen = gesamtEinnahmen,
            GesamtAusgaben = gesamtAusgaben,
            Bilanz = gesamtEinnahmen - gesamtAusgaben,
            KategorieAusgaben = kategorieAusgaben,
            KategorieEinnahmen = kategorieEinnahmen
        };
    }

    private static bool OccursInRange(RecurringTransaction da, DateTime rangeStart, DateTime rangeEnd)
    {
        if (da.Startdatum > rangeEnd) return false;
        if (da.Enddatum.HasValue && da.Enddatum.Value < rangeStart) return false;

        var candidate = da.LetzteAusfuehrung ?? da.Startdatum;
        if (candidate < da.Startdatum) candidate = da.Startdatum;

        var safety = 0;
        while (candidate < rangeStart && safety++ < 1000)
        {
            candidate = GetNextInstance(da, candidate);
        }

        return candidate >= rangeStart && candidate <= rangeEnd;
    }

    private static DateTime GetNextInstance(RecurringTransaction recurring, DateTime fromDate)
    {
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

public class DashboardMonthData
{
    public bool IstPrognose { get; set; }
    public decimal GesamtEinnahmen { get; set; }
    public decimal GesamtAusgaben { get; set; }
    public decimal Bilanz { get; set; }
    public List<CategorySummary> KategorieAusgaben { get; set; } = new List<CategorySummary>();
    public List<CategorySummary> KategorieEinnahmen { get; set; } = new List<CategorySummary>();
}