using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Resources.Strings;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class AccountDetailViewModel(
    SaveAccountDetailUseCase saveAccountDetailUseCase,
    INavigationService navigationService,
    ILocalizationService localizationService,
    IDialogService dialogService,
    ILogger<AccountDetailViewModel>? logger = null) : ObservableObject, IApplyQueryAttributes
{
    private readonly SaveAccountDetailUseCase _saveAccountDetailUseCase = saveAccountDetailUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILogger<AccountDetailViewModel>? _logger = logger;
    private Account? _existingAccount;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private AccountType type = AccountType.Girokonto;

    [ObservableProperty]
    private bool isArchived;

    [ObservableProperty]
    private AccountTypeOption? selectedTypeOption;

    [ObservableProperty]
    private string openingBalanceText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpeningBalanceDate))]
    private bool useOpeningBalanceDate;

    [ObservableProperty]
    private DateTime openingBalanceDate = DateTime.Today;

    public bool HasOpeningBalanceDate => UseOpeningBalanceDate;

    public string PageTitle => _existingAccount == null
        ? _loc.GetString(ResourceKeys.Title_KontoHinzufuegen)
        : _loc.GetString(ResourceKeys.Title_KontoBearbeiten);

    public bool IsSystemAccount => _existingAccount?.IsSystemAccount == true;
    public bool CanArchive => !IsSystemAccount;
    public string ArchiveStatusText => IsArchived
        ? _loc.GetString(ResourceKeys.Lbl_Archiviert)
        : _loc.GetString(ResourceKeys.Lbl_Aktiv);
    public string SystemAccountHint => IsSystemAccount
        ? _loc.GetString(ResourceKeys.Hint_Systemkonto)
        : string.Empty;

    partial void OnIsArchivedChanged(bool value) => OnPropertyChanged(nameof(ArchiveStatusText));

    public List<AccountTypeOption> VerfuegbareTypen =>
    [
        new(AccountType.Girokonto, _loc.GetString("AccountType_Girokonto")),
        new(AccountType.Tagesgeld, _loc.GetString("AccountType_Tagesgeld")),
        new(AccountType.Kreditkarte, _loc.GetString("AccountType_Kreditkarte")),
        new(AccountType.Bargeld, _loc.GetString("AccountType_Bargeld")),
        new(AccountType.Depot, _loc.GetString("AccountType_Depot")),
        new(AccountType.Sonstiges, _loc.GetString("AccountType_Sonstiges"))
    ];

    public Account? Account
    {
        set
        {
            if (value != null)
            {
                _existingAccount = value;
                Name = value.Name;
                Type = value.Type;
                IsArchived = value.IsArchived;
                OpeningBalanceText = value.OpeningBalance.ToString("F2", CultureInfo.CurrentCulture);
                UseOpeningBalanceDate = value.OpeningBalanceDate.HasValue;
                OpeningBalanceDate = value.OpeningBalanceDate ?? DateTime.Today;
                SelectedTypeOption = VerfuegbareTypen.FirstOrDefault(t => t.Value == value.Type);
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(IsSystemAccount));
                OnPropertyChanged(nameof(CanArchive));
                OnPropertyChanged(nameof(SystemAccountHint));
                OnPropertyChanged(nameof(ArchiveStatusText));
            }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Account", out var val) && val is Account a)
            Account = a;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        if (!TryParseOpeningBalance(out var openingBalance))
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_UngueltigerBetrag),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            Type = SelectedTypeOption?.Value ?? Type;
            var openingBalanceDate = UseOpeningBalanceDate ? OpeningBalanceDate : (DateTime?)null;
            await _saveAccountDetailUseCase.ExecuteAsync(
                _existingAccount, Name, Type, IsArchived, openingBalance, openingBalanceDate);
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AccountDetailViewModel: Save failed");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    private bool TryParseOpeningBalance(out decimal openingBalance)
    {
        if (string.IsNullOrWhiteSpace(OpeningBalanceText))
        {
            openingBalance = 0m;
            return true;
        }

        return decimal.TryParse(
            OpeningBalanceText,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out openingBalance);
    }
}

public sealed record AccountTypeOption(AccountType Value, string DisplayName);
