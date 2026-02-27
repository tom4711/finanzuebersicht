using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Transaction), "Transaction")]
public partial class TransactionDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private Transaction? _existingTransaction;

    [ObservableProperty]
    private string betragText = string.Empty;

    [ObservableProperty]
    private string titel = string.Empty;

    [ObservableProperty]
    private DateTime datum = DateTime.Today;

    [ObservableProperty]
    private Category? selectedKategorie;

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    public Transaction? Transaction
    {
        set
        {
            if (value != null)
            {
                _existingTransaction = value;
                BetragText = value.Betrag.ToString("F2",
                    System.Globalization.CultureInfo.CurrentCulture);
                Titel = value.Titel;
                Datum = value.Datum;
                Typ = value.Typ;

                // Kategorie nach Laden setzen
                _ = SetKategorieAsync(value.KategorieId);
            }
        }
    }

    public TransactionDetailViewModel(IDataService dataService)
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

        var transaction = _existingTransaction ?? new Transaction();
        transaction.Betrag = betrag;
        transaction.Titel = Titel;
        transaction.Datum = Datum;
        transaction.KategorieId = SelectedKategorie.Id;
        transaction.Typ = Typ;

        await _dataService.SaveTransactionAsync(transaction);
        await Shell.Current.GoToAsync("..");
    }

    private async Task SetKategorieAsync(string kategorieId)
    {
        if (Kategorien.Count == 0)
            await LoadKategorien();

        SelectedKategorie = Kategorien.FirstOrDefault(k => k.Id == kategorieId);
    }
}
