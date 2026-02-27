using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringTransaction), "RecurringTransaction")]
public partial class RecurringTransactionDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private RecurringTransaction? _existing;

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
    private bool hatEnddatum;

    [ObservableProperty]
    private DateTime enddatumWert = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private bool aktiv = true;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    public RecurringTransaction? RecurringTransaction
    {
        set
        {
            if (value != null)
            {
                _existing = value;
                BetragText = value.Betrag.ToString("F2",
                    System.Globalization.CultureInfo.CurrentCulture);
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

    public RecurringTransactionDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        var liste = await _dataService.GetCategoriesAsync();
        Kategorien = new ObservableCollection<Category>(liste);
    }

    [RelayCommand]
    private void SetTyp(string typName)
    {
        Typ = typName == "Einnahme" ? TransactionType.Einnahme : TransactionType.Ausgabe;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!decimal.TryParse(BetragText,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture,
                out var betrag) || betrag <= 0)
            return;

        if (string.IsNullOrWhiteSpace(Titel)) return;
        if (SelectedKategorie == null) return;

        var recurring = _existing ?? new RecurringTransaction();
        recurring.Betrag = betrag;
        recurring.Titel = Titel;
        recurring.KategorieId = SelectedKategorie.Id;
        recurring.Typ = Typ;
        recurring.Startdatum = Startdatum;
        recurring.Enddatum = HatEnddatum ? EnddatumWert : null;
        recurring.Aktiv = Aktiv;

        await _dataService.SaveRecurringTransactionAsync(recurring);
        await Shell.Current.GoToAsync("..");
    }

    private async Task SetKategorieAsync(string kategorieId)
    {
        if (Kategorien.Count == 0)
            await LoadKategorien();

        SelectedKategorie = Kategorien.FirstOrDefault(k => k.Id == kategorieId);
    }
}
