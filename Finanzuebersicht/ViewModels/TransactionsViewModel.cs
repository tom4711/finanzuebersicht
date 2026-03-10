using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;
using Microsoft.Maui.Storage;
using System.Linq;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Core.Services;

namespace Finanzuebersicht.ViewModels;

public partial class TransactionsViewModel : MonthNavigationViewModel
{
    private readonly DeleteTransactionUseCase _deleteTransactionUseCase;
    private readonly LoadTransactionsMonthUseCase _loadTransactionsMonthUseCase;
    private readonly INavigationService _navigationService;
    private readonly ImportService _importService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<TransactionsViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    public TransactionsViewModel(
        DeleteTransactionUseCase deleteTransactionUseCase,
        LoadTransactionsMonthUseCase loadTransactionsMonthUseCase,
        INavigationService navigationService,
        ImportService importService,
        IDialogService dialogService,
        ILogger<TransactionsViewModel> logger)
    {
        _deleteTransactionUseCase = deleteTransactionUseCase;
        _loadTransactionsMonthUseCase = loadTransactionsMonthUseCase;
        _navigationService = navigationService;
        _importService = importService;
        _dialogService = dialogService;
        _logger = logger;
    }

    protected override async Task OnMonthChangedAsync() => await LoadTransaktionen();

    [RelayCommand]
    private async Task LoadTransaktionen()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var data = await _loadTransactionsMonthUseCase.ExecuteAsync(AktuellerMonat);
            TransaktionsGruppen = new ObservableCollection<TransactionGroup>(data.Gruppen);
            Converters.KategorieIdToIconConverter.SetCache(data.IconMap);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTransaktion(Transaction transaktion)
    {
        await _deleteTransactionUseCase.ExecuteAsync(transaktion.Id);
        foreach (var gruppe in TransaktionsGruppen)
        {
            if (gruppe.Remove(transaktion))
            {
                if (gruppe.Count == 0)
                    TransaktionsGruppen.Remove(gruppe);
                break;
            }
        }
    }

    [RelayCommand]
    private async Task GoToDetail(Transaction? transaktion)
    {
        var parameter = new Dictionary<string, object>();
        if (transaktion != null)
            parameter["Transaction"] = transaktion;

        await _navigationService.GoToAsync(nameof(TransactionDetailPage), parameter);
    }

    [RelayCommand]
    private async Task ImportCsv()
    {
        // Defensive checks to avoid NullReferenceExceptions when DI failed
        if (_importService == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowAlertAsync("Import fehlgeschlagen", "ImportService nicht verfügbar.", "OK");
            else
                await App.Current.MainPage.DisplayAlert("Import fehlgeschlagen", "ImportService nicht verfügbar.", "OK");
            return;
        }

        try
        {
            var result = await FilePicker.PickAsync();
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            var imported = await _importService.ImportFromCsvAsync(stream);
            var count = imported?.Count() ?? 0;

            if (_dialogService != null)
                await _dialogService.ShowAlertAsync("Import abgeschlossen", $"Importiert: {count} Transaktionen", "OK");
            else
                await App.Current.MainPage.DisplayAlert("Import abgeschlossen", $"Importiert: {count} Transaktionen", "OK");

            await LoadTransaktionen();

            // notify other parts of the app (dashboard/year views) that data changed
            try { App.DataChanged?.Invoke(); } catch { }
        }
        catch (System.Exception ex)
        {
            // Log full exception for debugging
            try
            {
                _logger?.LogError(ex, "ImportCsv failed");
            }
            catch { /* swallow logger exceptions */ }

            // Ensure we don't call a null dialog service in the catch
            var msg = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : string.Empty);
            if (_dialogService != null)
                await _dialogService.ShowAlertAsync("Fehler beim Import", msg, "OK");
            else
                await App.Current.MainPage.DisplayAlert("Fehler beim Import", msg, "OK");
        }
    }
}
