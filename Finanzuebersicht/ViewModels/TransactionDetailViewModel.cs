using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Transaction), "Transaction")]
public partial class TransactionDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private Transaction? _existingTransaction;
    private readonly ILocalizationService _loc;

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

    public TransactionDetailViewModel(IDataService dataService, ILocalizationService localizationService)
    {
        _dataService = dataService;
        _loc = localizationService;
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
        try
        {
            // Validierung
            if (!decimal.TryParse(BetragText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture,
                    out var betrag))
            {
                await MainThread.InvokeOnMainThreadAsync(() => 
                    Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_UngueltigerBetrag), _loc.GetString(ResourceKeys.Btn_OK)));
                return;
            }

            if (betrag <= 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() => 
                    Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_BetragGroesserNull), _loc.GetString(ResourceKeys.Btn_OK)));
                return;
            }

            if (string.IsNullOrWhiteSpace(Titel))
            {
                await MainThread.InvokeOnMainThreadAsync(() => 
                    Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_TitelErforderlich), _loc.GetString(ResourceKeys.Btn_OK)));
                return;
            }

            if (SelectedKategorie == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() => 
                    Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_KategorieErforderlich), _loc.GetString(ResourceKeys.Btn_OK)));
                return;
            }

            var transaction = _existingTransaction ?? new Transaction();
            transaction.Betrag = betrag;
            transaction.Titel = Titel;
            transaction.Datum = Datum;
            transaction.KategorieId = SelectedKategorie.Id;
            transaction.Typ = Typ;

            await _dataService.SaveTransactionAsync(transaction);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving transaction: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() => 
                Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message), _loc.GetString(ResourceKeys.Btn_OK)));
        }
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
