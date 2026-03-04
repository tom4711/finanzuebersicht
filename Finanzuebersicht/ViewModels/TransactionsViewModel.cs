using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;

namespace Finanzuebersicht.ViewModels;

public partial class TransactionsViewModel : MonthNavigationViewModel
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly LoadTransactionsMonthUseCase _loadTransactionsMonthUseCase;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    public TransactionsViewModel(
        ITransactionRepository transactionRepository,
        LoadTransactionsMonthUseCase loadTransactionsMonthUseCase,
        INavigationService navigationService)
    {
        _transactionRepository = transactionRepository;
        _loadTransactionsMonthUseCase = loadTransactionsMonthUseCase;
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
