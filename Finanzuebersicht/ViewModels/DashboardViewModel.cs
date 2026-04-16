using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Core.Services;
namespace Finanzuebersicht.ViewModels;
public partial class DashboardViewModel : MonthNavigationViewModel
{
    private readonly LoadDashboardMonthUseCase _loadDashboardMonthUseCase;
    private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;
    private readonly LoadForecastUseCase _loadForecastUseCase;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IClock _clock;

    // --- Monatsansicht ---

    [ObservableProperty]
    private decimal gesamtEinnahmen;

    [ObservableProperty]
    private decimal gesamtAusgaben;

    [ObservableProperty]
    private decimal bilanz;

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieAusgaben = [];

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieEinnahmen = [];

    [ObservableProperty]
    private bool istPrognose;

    [ObservableProperty]
    private decimal forecastTotal;

    [ObservableProperty]
    private bool hasForecast;

    // Trend Vormonatsvergleich
    [ObservableProperty]
    private decimal? trendProzent;

    [ObservableProperty]
    private string trendText = string.Empty;

    [ObservableProperty]
    private bool trendPositiv;

    // Für BarChart: Forecast-Balken und Budget-Linie
    [ObservableProperty]
    private int forecastBarMonth;

    [ObservableProperty]
    private decimal forecastBarValue;

    [ObservableProperty]
    private decimal jahrBudgetTotal;

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

    private int _aktuellesJahr;

    public DashboardViewModel(
        LoadDashboardMonthUseCase loadDashboardMonthUseCase,
        LoadDashboardYearUseCase loadDashboardYearUseCase,
        LoadForecastUseCase loadForecastUseCase,
        IBudgetRepository budgetRepository,
        IClock? clock = null) : base(clock)
    {
        _clock = clock ?? SystemClock.Instance;
        _aktuellesJahr = _clock.Today.Year;
        _loadDashboardMonthUseCase = loadDashboardMonthUseCase;
        _loadDashboardYearUseCase = loadDashboardYearUseCase;
        _loadForecastUseCase = loadForecastUseCase;
        _budgetRepository = budgetRepository;
        UpdateJahrAnzeige();
    }

    protected override async Task OnMonthChangedAsync() => await LoadDashboard();

    [RelayCommand]
    private async Task LoadDashboard()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            if (IsMonthView)
            {
                await LadeMonatAsync();
            }
            else
            {
                await LadeJahrAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LadeMonatAsync()
    {
        var data = await _loadDashboardMonthUseCase.ExecuteAsync(AktuellerMonat, _clock.Today);

        IstPrognose = data.IstPrognose;
        GesamtEinnahmen = data.GesamtEinnahmen;
        GesamtAusgaben = data.GesamtAusgaben;
        Bilanz = data.Bilanz;
        KategorieAusgaben = new ObservableCollection<CategorySummary>(data.KategorieAusgaben);
        KategorieEinnahmen = new ObservableCollection<CategorySummary>(data.KategorieEinnahmen);

        // Vormonatsvergleich für Trend-Indikator
        var prevMonth = AktuellerMonat.AddMonths(-1);
        var prevData = await _loadDashboardMonthUseCase.ExecuteAsync(prevMonth, _clock.Today);
        if (prevData.GesamtAusgaben > 0)
        {
            var pct = (GesamtAusgaben - prevData.GesamtAusgaben) / prevData.GesamtAusgaben * 100;
            TrendProzent = pct;
            TrendPositiv = pct < 0; // weniger Ausgaben = positiv (grün)
            TrendText = pct >= 0
                ? $"↑ +{pct:F0} %"
                : $"↓ {pct:F0} %";
        }
        else
        {
            TrendProzent = null;
            TrendText = string.Empty;
        }

        // Load forecast for current month only (not for past/future navigation)
        var today = _clock.Today;
        var isCurrentMonth = AktuellerMonat.Year == today.Year && AktuellerMonat.Month == today.Month;
        if (isCurrentMonth)
        {
            var nextMonth = AktuellerMonat.AddMonths(1);
            var forecast = await _loadForecastUseCase.ExecuteAsync(nextMonth.Year, nextMonth.Month);
            ForecastTotal = forecast.ForecastedTotal;
            HasForecast = forecast.ForecastedTotal > 0;
        }
        else
        {
            HasForecast = false;
        }
    }

    private async Task LadeJahrAsync()
    {
        var data = await _loadDashboardYearUseCase.ExecuteAsync(_aktuellesJahr);
        JahrGesamtAusgaben = data.GesamtAusgaben;
        JahrMonate = data.Monate;
        JahrKategorien = new ObservableCollection<CategorySummary>(data.Kategorien);

        // Monatliches Gesamtbudget für Budget-Referenzlinie im Bar-Chart
        var budgets = await _budgetRepository.GetBudgetsAsync();
        JahrBudgetTotal = budgets
            .Where(b => b.Monat == null && b.Jahr == null && b.Betrag > 0)
            .Sum(b => b.Betrag);

        // Forecast-Balken für nächsten Monat (nur im aktuellen Jahr)
        var today = _clock.Today;
        if (_aktuellesJahr == today.Year)
        {
            var nextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);
            if (nextMonth.Year == _aktuellesJahr)
            {
                var forecast = await _loadForecastUseCase.ExecuteAsync(nextMonth.Year, nextMonth.Month);
                ForecastBarMonth = nextMonth.Month;
                ForecastBarValue = forecast.ForecastedTotal;
            }
            else
            {
                ForecastBarMonth = 0;
                ForecastBarValue = 0;
            }
        }
        else
        {
            ForecastBarMonth = 0;
            ForecastBarValue = 0;
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

    private void UpdateJahrAnzeige()
    {
        JahrAnzeige = _aktuellesJahr.ToString();
    }
}
