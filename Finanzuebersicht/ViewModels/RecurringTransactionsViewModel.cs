using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

public partial class RecurringTransactionsViewModel : ObservableObject
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<RecurringTransaction> dauerauftraege = [];

    [ObservableProperty]
    private bool isLoading;

    public RecurringTransactionsViewModel(
        IRecurringTransactionRepository recurringTransactionRepository,
        ILocalizationService localizationService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
        _loc = localizationService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadDauerauftraege()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
            Dauerauftraege = new ObservableCollection<RecurringTransaction>(
                liste.OrderByDescending(d => d.Aktiv).ThenBy(d => d.Titel));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleAktiv(RecurringTransaction dauerauftrag)
    {
        dauerauftrag.Aktiv = !dauerauftrag.Aktiv;
        await _recurringTransactionRepository.SaveRecurringTransactionAsync(dauerauftrag);
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

        await _recurringTransactionRepository.DeleteRecurringTransactionAsync(dauerauftrag.Id);
        Dauerauftraege.Remove(dauerauftrag);
    }

    [RelayCommand]
    private async Task GoToDetail(RecurringTransaction? dauerauftrag)
    {
        var parameter = new Dictionary<string, object>();
        if (dauerauftrag != null)
            parameter["RecurringTransaction"] = dauerauftrag;

        await _navigationService.GoToAsync(nameof(RecurringTransactionDetailPage), parameter);
    }
}
