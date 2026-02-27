using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;

namespace Finanzuebersicht.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string monatAnzeige = string.Empty;

    private DateTime _aktuellerMonat = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public TransactionsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        UpdateMonatAnzeige();
    }

    [RelayCommand]
    private async Task LoadTransaktionen()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var von = _aktuellerMonat;
            var bis = _aktuellerMonat.AddMonths(1).AddDays(-1);
            var liste = await _dataService.GetTransactionsAsync(von, bis);

            var gruppen = liste
                .GroupBy(t => t.Datum.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroup(g.Key, g.OrderByDescending(t => t.Datum)))
                .ToList();

            TransaktionsGruppen = new ObservableCollection<TransactionGroup>(gruppen);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTransaktion(Transaction transaktion)
    {
        await _dataService.DeleteTransactionAsync(transaktion.Id);
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

        await Shell.Current.GoToAsync(nameof(TransactionDetailPage), parameter);
    }

    [RelayCommand]
    private async Task NextMonth()
    {
        _aktuellerMonat = _aktuellerMonat.AddMonths(1);
        UpdateMonatAnzeige();
        await LoadTransaktionen();
    }

    [RelayCommand]
    private async Task PreviousMonth()
    {
        _aktuellerMonat = _aktuellerMonat.AddMonths(-1);
        UpdateMonatAnzeige();
        await LoadTransaktionen();
    }

    private void UpdateMonatAnzeige()
    {
        MonatAnzeige = _aktuellerMonat.ToString("MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
