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

public partial class TransactionsViewModel(
    DeleteTransactionUseCase deleteTransactionUseCase,
    LoadTransactionsMonthUseCase loadTransactionsMonthUseCase,
    INavigationService navigationService,
    ImportService importService,
    IDialogService dialogService,
    ILocalizationService localizationService,
    ILogger<TransactionsViewModel> logger) : MonthNavigationViewModel
{
    private readonly DeleteTransactionUseCase _deleteTransactionUseCase = deleteTransactionUseCase;
    private readonly LoadTransactionsMonthUseCase _loadTransactionsMonthUseCase = loadTransactionsMonthUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ImportService _importService = importService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly ILogger<TransactionsViewModel> _logger = logger;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    protected override async Task OnMonthChangedAsync() => await LoadTransaktionen();

    [RelayCommand]
    private async Task LoadTransaktionen()
    {
        if (IsLoading)
        {
            return;
        }

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
                {
                    TransaktionsGruppen.Remove(gruppe);
                }

                break;
            }
        }
    }

    [RelayCommand]
    private async Task GoToDetail(Transaction? transaktion)
    {
        try
        {
            _logger?.LogDebug("GoToDetail called for transaction {Id}", transaktion?.Id ?? "(new)");
            try { Finanzuebersicht.Core.Services.FileLogger.Append("TransactionsViewModel", $"GoToDetail called for {transaktion?.Id ?? "(new)"}"); } catch { }

            var parameter = new Dictionary<string, object>();
            if (transaktion != null)
            {
                parameter["Transaction"] = transaktion;
            }

            if (_navigationService == null)
            {
                _logger?.LogError("GoToDetail: navigation service is null");
                try { Finanzuebersicht.Core.Services.FileLogger.Append("TransactionsViewModel", "navigation service is null"); } catch { }
                return;
            }

            await _navigationService.GoToAsync(nameof(TransactionDetailPage), parameter);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GoToDetail failed");
            try { Finanzuebersicht.Core.Services.FileLogger.Append("TransactionsViewModel", "GoToDetail failed", ex); } catch { }
        }
    }

    [RelayCommand]
    private async Task ImportCsv()
    {
        // Defensive checks to avoid NullReferenceExceptions when DI failed
        if (_importService == null)
        {
            var title = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportFehlgeschlagen_Title) ?? "Import fehlgeschlagen";
            var msg = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportServiceNichtVerfuegbar) ?? "ImportService nicht verfügbar.";
            var ok = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            await _dialogService.ShowAlertAsync(title, msg, ok);

            return;
        }

        try
        {
            var result = await FilePicker.PickAsync();
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            var imported = await _importService.ImportFromCsvAsync(stream);
            var count = imported?.Count() ?? 0;

            var titleDone = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportAbgeschlossen_Title) ?? "Import abgeschlossen";
            var importedMsg = string.Format(_loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportiertCount) ?? "Importiert: {0} Transaktionen", count);
            var okBtn = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            await _dialogService.ShowAlertAsync(titleDone, importedMsg, okBtn);

            await LoadTransaktionen();

            // notify other parts of the app (dashboard/year views) that data changed
            try { App.NotifyDataChanged(); } catch { }
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
            var errTitle = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportFehler_Title) ?? "Fehler beim Import";
            var okError = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            await _dialogService.ShowAlertAsync(errTitle, msg, okError);
        }
    }
}
