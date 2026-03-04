using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Transaction), "Transaction")]
public partial class TransactionDetailViewModel : ObservableObject
{
    private readonly SaveTransactionDetailUseCase _saveTransactionDetailUseCase;
    private readonly LoadTransactionDetailDataUseCase _loadTransactionDetailDataUseCase;
    private readonly ITransactionValidationService _validationService;
    private Transaction? _existingTransaction;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

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

    public TransactionDetailViewModel(
        SaveTransactionDetailUseCase saveTransactionDetailUseCase,
        LoadTransactionDetailDataUseCase loadTransactionDetailDataUseCase,
        ITransactionValidationService validationService,
        ILocalizationService localizationService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _saveTransactionDetailUseCase = saveTransactionDetailUseCase;
        _loadTransactionDetailDataUseCase = loadTransactionDetailDataUseCase;
        _validationService = validationService;
        _loc = localizationService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        var data = await _loadTransactionDetailDataUseCase.ExecuteAsync(_existingTransaction?.KategorieId);
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
        try
        {
            if (!_validationService.TryValidate(
                    BetragText,
                    Titel,
                    SelectedKategorie != null,
                    System.Globalization.CultureInfo.CurrentCulture,
                    out var betrag,
                    out var error))
            {
                var message = error switch
                {
                    TransactionInputError.InvalidAmountFormat => _loc.GetString(ResourceKeys.Err_UngueltigerBetrag),
                    TransactionInputError.AmountMustBePositive => _loc.GetString(ResourceKeys.Err_BetragGroesserNull),
                    TransactionInputError.TitleRequired => _loc.GetString(ResourceKeys.Err_TitelErforderlich),
                    TransactionInputError.CategoryRequired => _loc.GetString(ResourceKeys.Err_KategorieErforderlich),
                    _ => _loc.GetString(ResourceKeys.Err_UngueltigerBetrag)
                };

                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Err_Titel),
                    message,
                    _loc.GetString(ResourceKeys.Btn_OK));
                return;
            }

            await _saveTransactionDetailUseCase.ExecuteAsync(
                _existingTransaction,
                betrag,
                Titel,
                Datum,
                SelectedKategorie!.Id,
                Typ);
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving transaction: {ex.Message}");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
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
