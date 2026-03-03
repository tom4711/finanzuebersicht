using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;

namespace Finanzuebersicht.ViewModels;

public partial class TransactionsViewModel : MonthNavigationViewModel
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    public TransactionsViewModel(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        INavigationService navigationService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _navigationService = navigationService;
    }

    protected override async Task OnMonthChangedAsync() => await LoadTransaktionen();

    [RelayCommand]
    private async Task LoadTransaktionen()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var von = AktuellerMonat;
            var bis = AktuellerMonat.AddMonths(1).AddDays(-1);
            var liste = await _transactionRepository.GetTransactionsAsync(von, bis);

            var gruppen = liste
                .GroupBy(t => t.Datum.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroup(g.Key, g.OrderByDescending(t => t.Datum)))
                .ToList();

            TransaktionsGruppen = new ObservableCollection<TransactionGroup>(gruppen);

            // Kategorie-Icon-Cache für Converter aktualisieren
            var categories = await _categoryRepository.GetCategoriesAsync();
            Converters.KategorieIdToIconConverter.SetCache(
                categories.ToDictionary(c => c.Id, c => c.Icon ?? "📁"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTransaktion(Transaction transaktion)
    {
        await _transactionRepository.DeleteTransactionAsync(transaktion.Id);
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
}
