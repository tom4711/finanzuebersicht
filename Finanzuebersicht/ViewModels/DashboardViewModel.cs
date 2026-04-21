using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;
public partial class DashboardViewModel : MonthNavigationViewModel
{
    private readonly LoadDashboardMonthUseCase _loadDashboardMonthUseCase;
    private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;
    private readonly LoadForecastUseCase _loadForecastUseCase;
    private readonly GetDueRecurringWithHintsUseCase _getDueRecurringUseCase;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IReportingService _reportingService;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly IClock _clock;

    // --- Monatsansicht ---

    [ObservableProperty]
    private decimal gesamtEinnahmen;

    [ObservableProperty]
    private decimal gesamtAusgaben;

    [ObservableProperty]
    private decimal bilanz;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMonthData))]
    private ObservableCollection<CategorySummary> kategorieAusgaben = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMonthData))]
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
    [NotifyPropertyChangedFor(nameof(HasYearData))]
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

    public bool HasMonthData => KategorieAusgaben.Count > 0 || KategorieEinnahmen.Count > 0;

    public bool HasYearData => JahrGesamtAusgaben > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDueItems))]
    [NotifyPropertyChangedFor(nameof(DueRecurringText))]
    private int dueRecurringCount;

    public bool HasDueItems => DueRecurringCount > 0;

    public string DueRecurringText => DueRecurringCount == 1
        ? _loc.GetString(ResourceKeys.Lbl_DauerauftraegeFaellig_Singular, DueRecurringCount)
        : _loc.GetString(ResourceKeys.Lbl_DauerauftraegeFaellig, DueRecurringCount);

    private int _aktuellesJahr;

    public DashboardViewModel(
        LoadDashboardMonthUseCase loadDashboardMonthUseCase,
        LoadDashboardYearUseCase loadDashboardYearUseCase,
        LoadForecastUseCase loadForecastUseCase,
        GetDueRecurringWithHintsUseCase getDueRecurringUseCase,
        IBudgetRepository budgetRepository,
        IReportingService reportingService,
        ILocalizationService localizationService,
        INavigationService navigationService,
        IClock? clock = null) : base(clock)
    {
        _clock = clock ?? SystemClock.Instance;
        _aktuellesJahr = _clock.Today.Year;
        _loadDashboardMonthUseCase = loadDashboardMonthUseCase;
        _loadDashboardYearUseCase = loadDashboardYearUseCase;
        _loadForecastUseCase = loadForecastUseCase;
        _getDueRecurringUseCase = getDueRecurringUseCase;
        _budgetRepository = budgetRepository;
        _reportingService = reportingService;
        _loc = localizationService;
        _navigationService = navigationService;
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
            await LadeFaelligeDauerauftraegeAsync();
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

        // Vormonatsvergleich: nur Gesamt-Ausgaben via ReportingService (keine Kategorien nötig)
        var prevMonth = AktuellerMonat.AddMonths(-1);
        var prevSummary = await _reportingService.GetMonthSummaryAsync(prevMonth.Year, prevMonth.Month);
        if (prevSummary.Total > 0)
        {
            var pct = (GesamtAusgaben - prevSummary.Total) / prevSummary.Total * 100;
            TrendProzent = pct;
            TrendPositiv = pct < 0; // weniger Ausgaben = positiv (grün)
            TrendText = pct >= 0
                ? $"↑ +{pct:F0} %"
                : $"↓ {pct:F0} %";
        }
        else
        {
            TrendProzent = null;
            TrendPositiv = false;
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

        // Summe der Default-Kategorie-Budgets (monat=null, jahr=null) als Referenzlinie
        // Hinweis: Monat- oder jahresspezifische Budget-Overrides werden bewusst nicht einberechnet,
        // da die Linie einen stabilen Jahres-Richtwert darstellen soll.
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

    [RelayCommand(CanExecute = nameof(CanGoNextYear))]
    private async Task NextYear()
    {
        _aktuellesJahr++;
        UpdateJahrAnzeige();
        await LoadDashboard();
    }

    [RelayCommand(CanExecute = nameof(CanGoPreviousYear))]
    private async Task PreviousYear()
    {
        _aktuellesJahr--;
        UpdateJahrAnzeige();
        await LoadDashboard();
    }

    private bool CanGoNextYear() => _aktuellesJahr < _clock.Today.Year;

    private bool CanGoPreviousYear() => _aktuellesJahr > _clock.Today.Year - 30;

    private void UpdateJahrAnzeige()
    {
        JahrAnzeige = _aktuellesJahr.ToString();
        NextYearCommand.NotifyCanExecuteChanged();
        PreviousYearCommand.NotifyCanExecuteChanged();
    }

    private async Task LadeFaelligeDauerauftraegeAsync()
    {
        var items = await _getDueRecurringUseCase.ExecuteAsync(_clock.Today);
        DueRecurringCount = items.Count(i => i.Hint != null);
    }

    [RelayCommand]
    private async Task NavigateToDauerauftraege()
    {
        await _navigationService.GoToAsync("//RecurringTransactionsPage");
    }
}
