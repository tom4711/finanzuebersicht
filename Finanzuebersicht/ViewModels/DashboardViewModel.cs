using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private decimal gesamtEinnahmen;

    [ObservableProperty]
    private decimal gesamtAusgaben;

    [ObservableProperty]
    private decimal bilanz;

    [ObservableProperty]
    private string monatAnzeige = string.Empty;

    [ObservableProperty]
    private ObservableCollection<KategorieZusammenfassung> kategorieAusgaben = [];

    [ObservableProperty]
    private ObservableCollection<KategorieZusammenfassung> kategorieEinnahmen = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool istPrognose;

    private DateTime _aktuellerMonat = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private List<Category> _alleKategorien = [];

    public DashboardViewModel(IDataService dataService)
    {
        _dataService = dataService;
        UpdateMonatAnzeige();
    }

    [RelayCommand]
    private async Task LoadDashboard()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            _alleKategorien = await _dataService.GetCategoriesAsync();

            var von = _aktuellerMonat;
            var bis = _aktuellerMonat.AddMonths(1).AddDays(-1);
            var transaktionen = await _dataService.GetTransactionsAsync(von, bis);

            // Prognose: zukünftige Monate mit erwarteten Daueraufträgen ergänzen
            IstPrognose = _aktuellerMonat > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (IstPrognose)
            {
                var dauerauftraege = await _dataService.GetRecurringTransactionsAsync();
                foreach (var da in dauerauftraege.Where(d => d.Aktiv))
                {
                    if (da.Startdatum <= bis && (!da.Enddatum.HasValue || da.Enddatum.Value >= von))
                    {
                        // Nur hinzufügen wenn nicht schon eine generierte Transaktion existiert
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

            GesamtEinnahmen = transaktionen
                .Where(t => t.Typ == TransactionType.Einnahme)
                .Sum(t => t.Betrag);

            GesamtAusgaben = transaktionen
                .Where(t => t.Typ == TransactionType.Ausgabe)
                .Sum(t => t.Betrag);

            Bilanz = GesamtEinnahmen - GesamtAusgaben;

            // Kategorie-Aufschlüsselung Ausgaben
            var ausgabenGruppiert = transaktionen
                .Where(t => t.Typ == TransactionType.Ausgabe)
                .GroupBy(t => t.KategorieId)
                .Select(g => new KategorieZusammenfassung
                {
                    Kategorie = _alleKategorien.FirstOrDefault(k => k.Id == g.Key) ?? new Category { Name = "Unbekannt" },
                    Summe = g.Sum(t => t.Betrag),
                    Prozent = GesamtAusgaben > 0 ? (double)(g.Sum(t => t.Betrag) / GesamtAusgaben * 100) : 0
                })
                .OrderByDescending(k => k.Summe)
                .ToList();

            KategorieAusgaben = new ObservableCollection<KategorieZusammenfassung>(ausgabenGruppiert);

            // Kategorie-Aufschlüsselung Einnahmen
            var einnahmenGruppiert = transaktionen
                .Where(t => t.Typ == TransactionType.Einnahme)
                .GroupBy(t => t.KategorieId)
                .Select(g => new KategorieZusammenfassung
                {
                    Kategorie = _alleKategorien.FirstOrDefault(k => k.Id == g.Key) ?? new Category { Name = "Unbekannt" },
                    Summe = g.Sum(t => t.Betrag),
                    Prozent = GesamtEinnahmen > 0 ? (double)(g.Sum(t => t.Betrag) / GesamtEinnahmen * 100) : 0
                })
                .OrderByDescending(k => k.Summe)
                .ToList();

            KategorieEinnahmen = new ObservableCollection<KategorieZusammenfassung>(einnahmenGruppiert);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NextMonth()
    {
        _aktuellerMonat = _aktuellerMonat.AddMonths(1);
        UpdateMonatAnzeige();
        await LoadDashboard();
    }

    [RelayCommand]
    private async Task PreviousMonth()
    {
        _aktuellerMonat = _aktuellerMonat.AddMonths(-1);
        UpdateMonatAnzeige();
        await LoadDashboard();
    }

    private void UpdateMonatAnzeige()
    {
        MonatAnzeige = _aktuellerMonat.ToString("MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
