using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.SparZiele;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels;

public partial class SparZieleViewModel : ObservableObject
{
    private readonly LoadSparZieleUseCase _loadUseCase;
    private readonly SaveSparZielUseCase _saveUseCase;
    private readonly DeleteSparZielUseCase _deleteUseCase;

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
        DeleteSparZielUseCase deleteUseCase)
    {
        _loadUseCase = loadUseCase;
        _saveUseCase = saveUseCase;
        _deleteUseCase = deleteUseCase;
    }

    [RelayCommand]
    private async Task LoadSparZiele()
    {
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
        if (string.IsNullOrWhiteSpace(NeuerTitel) || NeuesZielBetrag <= 0) return;

        var ziel = new SparZiel
        {
            Titel = NeuerTitel,
            Icon = NeuerIcon,
            ZielBetrag = NeuesZielBetrag,
            AktuellerBetrag = NeuerAktuellerBetrag,
            Faelligkeitsdatum = NeueFaelligkeit
        };

        await _saveUseCase.ExecuteAsync(ziel);
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
