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
                    if (!transaktionen.Any(t => t.DauerauftragId == da.Id))
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
            .Sum(t => t.Betrag);

        var gesamtAusgaben = transaktionen
            .Where(t => t.Typ == TransactionType.Ausgabe)
            .Sum(t => t.Betrag);

        var kategorieAusgaben = transaktionen
            .Where(t => t.Typ == TransactionType.Ausgabe)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = kategorien.FirstOrDefault(k => k.Id == g.Key), Total = g.Sum(t => t.Betrag) })
            .Where(x => x.Cat != null)
            .Select(x => new CategorySummary
            {
                CategoryId = x.Key,
                CategoryName = x.Cat!.Name,
                Total = x.Total,
                Color = x.Cat.Color,
                Icon = x.Cat.Icon
            })
            .OrderByDescending(k => k.Total)
            .ToList();

        var kategorieEinnahmen = transaktionen
            .Where(t => t.Typ == TransactionType.Einnahme)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = kategorien.FirstOrDefault(k => k.Id == g.Key), Total = g.Sum(t => t.Betrag) })
            .Where(x => x.Cat != null)
            .Select(x => new CategorySummary
            {
                CategoryId = x.Key,
                CategoryName = x.Cat!.Name,
                Total = x.Total,
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
}

public class DashboardMonthData
{
    public bool IstPrognose { get; set; }
    public decimal GesamtEinnahmen { get; set; }
    public decimal GesamtAusgaben { get; set; }
    public decimal Bilanz { get; set; }
    public List<CategorySummary> KategorieAusgaben { get; set; } = [];
    public List<CategorySummary> KategorieEinnahmen { get; set; } = [];
}