using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.SparZiele;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class SparZieleViewModel : ObservableObject
{
    private readonly LoadSparZieleUseCase _loadUseCase;
    private readonly SaveSparZielUseCase _saveUseCase;
    private readonly DeleteSparZielUseCase _deleteUseCase;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _loc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private ObservableCollection<SparZielSummary> sparZiele = [];

    public bool IsEmpty => SparZiele.Count == 0;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string neuerTitel = string.Empty;

    [ObservableProperty]
    private string neuerIcon = "🎯";

    [ObservableProperty]
    private decimal neuesZielBetrag;

    [ObservableProperty]
    private decimal neuerAktuellerBetrag;

    [ObservableProperty]
    private DateTime? neueFaelligkeit;

    [ObservableProperty]
    private bool showAddForm;

    public SparZieleViewModel(
        LoadSparZieleUseCase loadUseCase,
        SaveSparZielUseCase saveUseCase,
        DeleteSparZielUseCase deleteUseCase,
        IDialogService dialogService,
        ILocalizationService localizationService)
    {
        _loadUseCase = loadUseCase;
        _saveUseCase = saveUseCase;
        _deleteUseCase = deleteUseCase;
        _dialogService = dialogService;
        _loc = localizationService;
    }

    [RelayCommand]
    private async Task LoadSparZiele()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var items = await _loadUseCase.ExecuteAsync();
            SparZiele = new ObservableCollection<SparZielSummary>(items);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        ShowAddForm = !ShowAddForm;
        if (ShowAddForm)
        {
            NeuerTitel = string.Empty;
            NeuerIcon = "🎯";
            NeuesZielBetrag = 0;
            NeuerAktuellerBetrag = 0;
            NeueFaelligkeit = null;
        }
    }

    [RelayCommand]
    private async Task SaveNewSparZiel()
    {
        if (string.IsNullOrWhiteSpace(NeuerTitel))
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_TitelErforderlich),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }
        if (NeuesZielBetrag <= 0)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_BetragGroesserNull),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }
        if (NeuerAktuellerBetrag < 0)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_UngueltigerBetrag),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            var ziel = new SparZiel
            {
                Titel = NeuerTitel,
                Icon = string.IsNullOrWhiteSpace(NeuerIcon) ? "🎯" : NeuerIcon,
                ZielBetrag = NeuesZielBetrag,
                AktuellerBetrag = NeuerAktuellerBetrag,
                Faelligkeitsdatum = NeueFaelligkeit
            };

            await _saveUseCase.ExecuteAsync(ziel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveNewSparZiel failed: {ex}");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        ShowAddForm = false;
        await LoadSparZieleCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task UpdateBetrag(SparZiel ziel)
    {
        await _saveUseCase.ExecuteAsync(ziel);
        await LoadSparZieleCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task DeleteSparZiel(string id)
    {
        await _deleteUseCase.ExecuteAsync(id);
        await LoadSparZieleCommand.ExecuteAsync(null);
    }
}
