using System;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadDashboardMonthUseCase(
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IRecurringTransactionRepository recurringTransactionRepository,
    IBudgetRepository budgetRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;
    private readonly IBudgetRepository _budgetRepository = budgetRepository;

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

        // Enrich with budget data
        var budgets = await _budgetRepository.GetBudgetsAsync() ?? [];
        foreach (var cs in kategorieAusgaben)
        {
            var budget = budgets.FirstOrDefault(b => b.KategorieId == cs.CategoryId && b.Jahr == aktuellerMonat.Year && b.Monat == aktuellerMonat.Month)
                ?? budgets.FirstOrDefault(b => b.KategorieId == cs.CategoryId && b.Jahr == null && b.Monat == aktuellerMonat.Month)
                ?? budgets.FirstOrDefault(b => b.KategorieId == cs.CategoryId && b.Jahr == null && b.Monat == null);
            if (budget != null)
                cs.BudgetBetrag = budget.Betrag;
        }

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
        // Grundlegende Plausibilitätsprüfung: Zeitraum vollständig vor/nach dem Dauerauftrag
        if (da.Startdatum > rangeEnd) return false;
        if (da.Enddatum.HasValue && da.Enddatum.Value < rangeStart) return false;

        // Starte bei der letzten Ausführung oder dem Startdatum
        var candidate = da.LetzteAusfuehrung ?? da.Startdatum;
        if (candidate < da.Startdatum) candidate = da.Startdatum;

        var safety = 0;

        // Iteriere über Basis-Vorkommen bis zum Ende des betrachteten Zeitraums
        while (candidate <= rangeEnd && safety++ < 1000)
        {
            // Nur innerhalb der generellen Lebensdauer des Dauerauftrags berücksichtigen
            if (candidate >= da.Startdatum && (!da.Enddatum.HasValue || candidate <= da.Enddatum.Value))
            {
                if (TryGetEffectiveDateWithExceptions(da, candidate, out var effectiveDate))
                {
                    if (effectiveDate >= rangeStart && effectiveDate <= rangeEnd)
                    {
                        return true;
                    }
                }
            }

            candidate = GetNextInstance(da, candidate);
        }

        return false;
    }

    private static bool TryGetEffectiveDateWithExceptions(RecurringTransaction recurring, DateTime baseDate, out DateTime effectiveDate)
    {
        // Standardfall: kein Eintrag in den Ausnahmen → Basisdatum ist das effektive Datum
        effectiveDate = baseDate.Date;

        // Wenn keine Ausnahmen definiert sind, direkt zurück
        if (recurring.Exceptions == null)
        {
            return true;
        }

        RecurringException? matchingException = null;

        foreach (var ex in recurring.Exceptions)
        {
            // Vergleich nur auf Datumsebene
            if (ex.InstanceDate.Date == baseDate.Date)
            {
                matchingException = ex;
                break;
            }
        }

        if (matchingException == null)
        {
            // Keine Ausnahme für dieses Vorkommen → Basisdatum bleibt gültig
            return true;
        }

        // Skip → dieses Vorkommen existiert nicht
        if (matchingException.Type == RecurringExceptionType.Skip)
        {
            return false;
        }

        // Shift → effektives Datum ist das hinterlegte Shift-Datum, falls vorhanden
        if (matchingException.Type == RecurringExceptionType.Shift && matchingException.ShiftToDate.HasValue)
        {
            effectiveDate = matchingException.ShiftToDate.Value.Date;
            return true;
        }

        // Fallback: falls Ausnahme-Typ unbekannt oder Shift ohne Ziel-Datum, Basisdatum verwenden
        return true;
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