using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringTransaction), "RecurringTransaction")]
public partial class RecurringTransactionDetailViewModel(
    SaveRecurringTransactionDetailUseCase saveRecurringTransactionDetailUseCase,
    LoadRecurringTransactionDetailDataUseCase loadRecurringTransactionDetailDataUseCase,
    ITransactionValidationService validationService,
    INavigationService navigationService) : ObservableObject
{
    private readonly SaveRecurringTransactionDetailUseCase _saveRecurringTransactionDetailUseCase = saveRecurringTransactionDetailUseCase;
    private readonly LoadRecurringTransactionDetailDataUseCase _loadRecurringTransactionDetailDataUseCase = loadRecurringTransactionDetailDataUseCase;
    private readonly ITransactionValidationService _validationService = validationService;
    private RecurringTransaction? _existing;
    private readonly INavigationService _navigationService = navigationService;

    [ObservableProperty]
    private string betragText = string.Empty;

    [ObservableProperty]
    private string titel = string.Empty;

    [ObservableProperty]
    private Category? selectedKategorie;

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

    [ObservableProperty]
    private DateTime startdatum = DateTime.Today;

    [ObservableProperty]
    private DateTime? enddatum;

    [ObservableProperty]
    private readonly bool hatEnddatum;

    [ObservableProperty]
    private DateTime enddatumWert = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private readonly bool aktiv = true;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    public RecurringTransaction? RecurringTransaction
    {
        set
        {
            if (value != null)
            {
                _existing = value;
                BetragText = value.Betrag.ToString("F2", System.Globalization.CultureInfo.CurrentCulture);
                Titel = value.Titel;
                Typ = value.Typ;
                Startdatum = value.Startdatum;
                Aktiv = value.Aktiv;

                if (value.Enddatum.HasValue)
                {
                    HatEnddatum = true;
                    EnddatumWert = value.Enddatum.Value;
                }

                _ = SetKategorieAsync(value.KategorieId);
            }
        }
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        var currentId = SelectedKategorie?.Id ?? _existing?.KategorieId;
        var data = await _loadRecurringTransactionDetailDataUseCase.ExecuteAsync(currentId);
        Kategorien = new ObservableCollection<Category>(data.Kategorien);
        SelectedKategorie = data.SelectedKategorie;
    }

    [RelayCommand]
    private void SetTyp(string typName)
    {
        Typ = typName == "Einnahme" ? TransactionType.Einnahme : TransactionType.Ausgabe;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!_validationService.TryValidate(
                BetragText,
                Titel,
                SelectedKategorie != null,
                System.Globalization.CultureInfo.CurrentCulture,
                out var betrag,
                out _))
            return;

        await _saveRecurringTransactionDetailUseCase.ExecuteAsync(
            _existing,
            betrag,
            Titel,
            SelectedKategorie!.Id,
            Typ,
            Startdatum,
            HatEnddatum ? EnddatumWert : null,
            Aktiv);
        await _navigationService.GoBackAsync();
    }

    private async Task SetKategorieAsync(string kategorieId)
    {
        try
        {
            if (Kategorien.Count == 0)
                await LoadKategorien();

            SelectedKategorie = Kategorien.FirstOrDefault(k => k.Id == kategorieId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Kategorie: {ex.Message}");
        }
    }
}
