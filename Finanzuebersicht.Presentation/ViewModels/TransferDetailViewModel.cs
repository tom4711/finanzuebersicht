using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Resources.Strings;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class TransferDetailViewModel(
    SaveTransferUseCase saveTransferUseCase,
    IAccountRepository accountRepository,
    INavigationService navigationService,
    IDialogService dialogService,
    ILocalizationService localizationService,
    ILogger<TransferDetailViewModel>? logger = null,
    Finanzuebersicht.Core.Services.IClock? clock = null) : ObservableObject, IAutoLoadViewModel
{
    private readonly SaveTransferUseCase _saveTransferUseCase = saveTransferUseCase;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly ILogger<TransferDetailViewModel>? _logger = logger;
    private readonly Finanzuebersicht.Core.Services.IClock _clock = clock ?? Finanzuebersicht.Core.Services.SystemClock.Instance;

    public System.Windows.Input.ICommand AutoLoadCommand => LoadAccountsCommand;

    [ObservableProperty]
    private ObservableCollection<Account> accounts = [];

    [ObservableProperty]
    private Account? sourceAccount;

    [ObservableProperty]
    private Account? targetAccount;

    [ObservableProperty]
    private string amountText = string.Empty;

    [ObservableProperty]
    private string title = "Umbuchung";

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private DateTime date = Finanzuebersicht.Core.Services.SystemClock.Instance.Today;

    [RelayCommand]
    private async Task LoadAccounts()
    {
        var accounts = await _accountRepository.GetAccountsAsync();
        var active = accounts.Where(a => !a.IsArchived).OrderBy(a => a.Name).ToList();
        Accounts = new ObservableCollection<Account>(active);

        SourceAccount = active.FirstOrDefault(a => a.SystemKey == Finanzuebersicht.Constants.SystemAccountKeys.Default)
            ?? active.FirstOrDefault();
        TargetAccount = active.FirstOrDefault(a => a.Id != SourceAccount?.Id);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var amount) || amount <= 0)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_BetragGroesserNull),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        if (SourceAccount == null || TargetAccount == null || SourceAccount.Id == TargetAccount.Id)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                "Bitte unterschiedliche Quell- und Zielkonten auswählen.",
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            await _saveTransferUseCase.ExecuteAsync(
                SourceAccount.Id,
                TargetAccount.Id,
                amount,
                Date,
                Title,
                Note);

            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving transfer");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }
}
