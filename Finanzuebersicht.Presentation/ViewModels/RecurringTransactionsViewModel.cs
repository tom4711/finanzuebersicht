using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class RecurringTransactionsViewModel(
    DeleteRecurringTransactionUseCase deleteRecurringTransactionUseCase,
    LoadRecurringTransactionsUseCase loadRecurringTransactionsUseCase,
    ToggleRecurringTransactionActiveUseCase toggleRecurringTransactionActiveUseCase,
    ILocalizationService localizationService,
    INavigationService navigationService,
    IDialogService dialogService) : ObservableObject, IAutoLoadViewModel
{
    private readonly DeleteRecurringTransactionUseCase _deleteRecurringTransactionUseCase = deleteRecurringTransactionUseCase;
    private readonly LoadRecurringTransactionsUseCase _loadRecurringTransactionsUseCase = loadRecurringTransactionsUseCase;
    private readonly ToggleRecurringTransactionActiveUseCase _toggleRecurringTransactionActiveUseCase = toggleRecurringTransactionActiveUseCase;
    private readonly ILocalizationService _loc = localizationService;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;

    public System.Windows.Input.ICommand AutoLoadCommand => LoadDauerauftraegeCommand;

    [ObservableProperty]
    private ObservableCollection<RecurringTransaction> dauerauftraege = [];

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task LoadDauerauftraege()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _loadRecurringTransactionsUseCase.ExecuteAsync();
            Dauerauftraege = new ObservableCollection<RecurringTransaction>(liste);
        }
        catch (Exception ex)
        {
            try { Finanzuebersicht.Services.FileLogger.Append("RecurringTransactionsViewModel", nameof(LoadDauerauftraege), ex); } catch { }
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_LadenFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleAktiv(RecurringTransaction dauerauftrag)
    {
        await _toggleRecurringTransactionActiveUseCase.ExecuteAsync(dauerauftrag);
        await LoadDauerauftraege();
    }

    [RelayCommand]
    private async Task DeleteDauerauftrag(RecurringTransaction dauerauftrag)
    {
        var bestaetigt = await _dialogService.ShowConfirmationAsync(
            _loc.GetString(ResourceKeys.Dlg_DauerauftragLoeschen),
            _loc.GetString(ResourceKeys.Dlg_DauerauftragLoeschenFrage, dauerauftrag.Titel),
            _loc.GetString(ResourceKeys.Btn_Ja), _loc.GetString(ResourceKeys.Btn_Nein));

        if (!bestaetigt) return;

        try
        {
            await _deleteRecurringTransactionUseCase.ExecuteAsync(dauerauftrag.Id);
            Dauerauftraege.Remove(dauerauftrag);
        }
        catch (Exception ex)
        {
            try { Finanzuebersicht.Services.FileLogger.Append("RecurringTransactionsViewModel", nameof(DeleteDauerauftrag), ex); } catch { }
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_LoeschenFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task GoToDetail(RecurringTransaction? dauerauftrag)
    {
        var parameter = new Dictionary<string, object>();
        if (dauerauftrag != null)
        {
            parameter["RecurringTransaction"] = dauerauftrag;
        }

        await _navigationService.GoToAsync(Routes.RecurringTransactionDetail, parameter);
    }
}
