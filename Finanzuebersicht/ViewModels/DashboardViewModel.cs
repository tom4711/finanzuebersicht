using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    // --- Monatsansicht ---

    [ObservableProperty]
    private decimal gesamtEinnahmen;

    [ObservableProperty]
    private decimal gesamtAusgaben;

    [ObservableProperty]
    private decimal bilanz;

    [ObservableProperty]
    private string monatAnzeige = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieAusgaben = [];

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieEinnahmen = [];

    [ObservableProperty]
    private bool istPrognose;

    // --- Jahresansicht ---

    [ObservableProperty]
    private string jahrAnzeige = string.Empty;

    [ObservableProperty]
    private decimal jahrGesamtAusgaben;

    [ObservableProperty]
    private ObservableCollection<CategorySummary> jahrKategorien = [];

    [ObservableProperty]
    private List<MonthSummary> jahrMonate = [];

    // --- Allgemein ---

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYearView))]
    private bool isMonthView = true;

    public bool IsYearView => !IsMonthView;

    private DateTime _aktuellerMonat = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private int _aktuellesJahr = DateTime.Today.Year;
    private List<Category> _alleKategorien = [];

    public DashboardViewModel(IDataService dataService)
    {
        _dataService = dataService;
        UpdateMonatAnzeige();
        UpdateJahrAnzeige();
    }

    [RelayCommand]
    private async Task LoadDashboard()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            if (IsMonthView)
                await LadeMonatAsync();
            else
                await LadeJahrAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LadeMonatAsync()
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

        var ausgabenGruppiert = transaktionen
            .Where(t => t.Typ == TransactionType.Ausgabe)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = _alleKategorien.FirstOrDefault(k => k.Id == g.Key), Total = g.Sum(t => t.Betrag) })
            .Where(x => x.Cat != null) // Transaktionen ohne gültige Kategorie nicht im Diagramm anzeigen
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

        KategorieAusgaben = new ObservableCollection<CategorySummary>(ausgabenGruppiert);

        var einnahmenGruppiert = transaktionen
            .Where(t => t.Typ == TransactionType.Einnahme)
            .GroupBy(t => t.KategorieId)
            .Select(g => new { Key = g.Key, Cat = _alleKategorien.FirstOrDefault(k => k.Id == g.Key), Total = g.Sum(t => t.Betrag) })
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

        KategorieEinnahmen = new ObservableCollection<CategorySummary>(einnahmenGruppiert);
    }

    private async Task LadeJahrAsync()
    {
        var summary = await _dataService.GetYearSummaryAsync(_aktuellesJahr);
        if (summary != null)
        {
            JahrGesamtAusgaben = summary.Total;
            JahrMonate = summary.Months;
            if (summary.ByCategory != null && summary.Total > 0)
            {
                foreach (var cat in summary.ByCategory)
                    cat.PercentageAmount = (cat.Total / summary.Total) * 100;
            }
            JahrKategorien = new ObservableCollection<CategorySummary>(summary.ByCategory ?? []);
        }
        else
        {
            JahrGesamtAusgaben = 0;
            JahrMonate = [];
            JahrKategorien = [];
        }
    }

    [RelayCommand]
    private async Task ZeigeMonate()
    {
        if (IsMonthView) return;
        IsMonthView = true;
        await LoadDashboard();
    }

    [RelayCommand]
    private async Task ZeigeJahr()
    {
        if (IsYearView) return;
        IsMonthView = false;
        await LoadDashboard();
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

    [RelayCommand]
    private async Task NextYear()
    {
        _aktuellesJahr++;
        UpdateJahrAnzeige();
        await LoadDashboard();
    }

    [RelayCommand]
    private async Task PreviousYear()
    {
        _aktuellesJahr--;
        UpdateJahrAnzeige();
        await LoadDashboard();
    }

    private void UpdateMonatAnzeige()
    {
        MonatAnzeige = _aktuellerMonat.ToString("MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }

    private void UpdateJahrAnzeige()
    {
        JahrAnzeige = _aktuellesJahr.ToString();
    }
}
