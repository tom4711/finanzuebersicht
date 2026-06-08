using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class CashflowViewModel(
    LoadCashflowOutlookUseCase loadCashflowOutlookUseCase,
    IAccountRepository accountRepository,
    ILocalizationService localizationService,
    IDialogService dialogService,
    ILogger<CashflowViewModel>? logger = null) : ObservableObject, IAutoLoadViewModel
{
    private readonly LoadCashflowOutlookUseCase _loadCashflowOutlookUseCase = loadCashflowOutlookUseCase;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ILocalizationService _loc = localizationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILogger<CashflowViewModel>? _logger = logger;

    public System.Windows.Input.ICommand AutoLoadCommand => LoadCashflowCommand;

    [ObservableProperty]
    private ObservableCollection<CashflowDayGroup> days = [];

    [ObservableProperty]
    private ObservableCollection<KategorieFilterItem> availableKonten = [];

    [ObservableProperty]
    private KategorieFilterItem? selectedKontoFilterItem;

    [ObservableProperty]
    private string? selectedAccountId;

    [ObservableProperty]
    private decimal projectedIncome;

    [ObservableProperty]
    private decimal projectedExpenses;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDays))]
    private bool hasData;

    public bool HasDays => Days.Count > 0;

    partial void OnSelectedKontoFilterItemChanged(KategorieFilterItem? value)
    {
        SelectedAccountId = value?.Id;
        _ = LoadCashflow();
    }

    [RelayCommand]
    private async Task LoadCashflow()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            await EnsureAccountFilterLoadedAsync();
            var data = await _loadCashflowOutlookUseCase.ExecuteAsync(accountId: SelectedAccountId);
            Days = new ObservableCollection<CashflowDayGroup>(data.Days);
            ProjectedIncome = data.ProjectedIncome;
            ProjectedExpenses = data.ProjectedExpenses;
            HasData = data.Days.Count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CashflowViewModel load failed");
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

    private async Task EnsureAccountFilterLoadedAsync()
    {
        if (AvailableKonten.Count > 0) return;

        var accounts = await _accountRepository.GetAccountsAsync();
        var items = new ObservableCollection<KategorieFilterItem>
        {
            new(null, _loc.GetString(ResourceKeys.Lbl_AlleKonten))
        };

        foreach (var account in accounts.Where(a => !a.IsArchived).OrderBy(a => a.Name))
            items.Add(new KategorieFilterItem(account.Id, account.Name));

        AvailableKonten = items;
        SelectedKontoFilterItem = items[0];
    }
}
