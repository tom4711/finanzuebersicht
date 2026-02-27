using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;

namespace Finanzuebersicht.ViewModels;

public partial class RecurringTransactionsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<RecurringTransaction> dauerauftraege = [];

    [ObservableProperty]
    private bool isLoading;

    public RecurringTransactionsViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoadDauerauftraege()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _dataService.GetRecurringTransactionsAsync();
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
        await _dataService.SaveRecurringTransactionAsync(dauerauftrag);
        await LoadDauerauftraege();
    }

    [RelayCommand]
    private async Task DeleteDauerauftrag(RecurringTransaction dauerauftrag)
    {
        await _dataService.DeleteRecurringTransactionAsync(dauerauftrag.Id);
        Dauerauftraege.Remove(dauerauftrag);
    }

    [RelayCommand]
    private async Task GoToDetail(RecurringTransaction? dauerauftrag)
    {
        var parameter = new Dictionary<string, object>();
        if (dauerauftrag != null)
            parameter["RecurringTransaction"] = dauerauftrag;

        await Shell.Current.GoToAsync(nameof(RecurringTransactionDetailPage), parameter);
    }
}
