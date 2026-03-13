using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class DashboardViewModel : MonthNavigationViewModel
{
    private readonly LoadDashboardMonthUseCase _loadDashboardMonthUseCase;
    private readonly LoadDashboardYearUseCase _loadDashboardYearUseCase;

    // --- Monatsansicht ---

    [ObservableProperty]
    private readonly decimal gesamtEinnahmen;

    [ObservableProperty]
    private readonly decimal gesamtAusgaben;

    [ObservableProperty]
    private readonly decimal bilanz;

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieAusgaben = [];

    [ObservableProperty]
    private ObservableCollection<CategorySummary> kategorieEinnahmen = [];

    [ObservableProperty]
    private readonly bool istPrognose;

    // --- Jahresansicht ---

    [ObservableProperty]
    private string jahrAnzeige = string.Empty;

    [ObservableProperty]
    private readonly decimal jahrGesamtAusgaben;

    [ObservableProperty]
    private ObservableCollection<CategorySummary> jahrKategorien = [];

    [ObservableProperty]
    private List<MonthSummary> jahrMonate = [];

    // --- Allgemein ---

    [ObservableProperty]
    private readonly bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYearView))]
    private readonly bool isMonthView = true;

    public bool IsYearView => !IsMonthView;

    private int _aktuellesJahr = DateTime.Today.Year;

    public DashboardViewModel(
        LoadDashboardMonthUseCase loadDashboardMonthUseCase,
        LoadDashboardYearUseCase loadDashboardYearUseCase)
    {
        _loadDashboardMonthUseCase = loadDashboardMonthUseCase;
        _loadDashboardYearUseCase = loadDashboardYearUseCase;
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
        var data = await _loadDashboardMonthUseCase.ExecuteAsync(AktuellerMonat, DateTime.Today);

        IstPrognose = data.IstPrognose;
        GesamtEinnahmen = data.GesamtEinnahmen;
        GesamtAusgaben = data.GesamtAusgaben;
        Bilanz = data.Bilanz;
        KategorieAusgaben = new ObservableCollection<CategorySummary>(data.KategorieAusgaben);
        KategorieEinnahmen = new ObservableCollection<CategorySummary>(data.KategorieEinnahmen);
    }

    private async Task LadeJahrAsync()
    {
        var data = await _loadDashboardYearUseCase.ExecuteAsync(_aktuellesJahr);
        JahrGesamtAusgaben = data.GesamtAusgaben;
        JahrMonate = data.Monate;
        JahrKategorien = new ObservableCollection<CategorySummary>(data.Kategorien);
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
