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
    ILogger<AccountDetailViewModel>? logger = null) : ObservableObject, IApplyQueryAttributes
{
    private readonly SaveAccountDetailUseCase _saveAccountDetailUseCase = saveAccountDetailUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ILocalizationService _loc = localizationService;
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

        Type = SelectedTypeOption?.Value ?? Type;
        await _saveAccountDetailUseCase.ExecuteAsync(_existingAccount, Name, Type, IsArchived);
        await _navigationService.GoBackAsync();
    }
}

public sealed record AccountTypeOption(AccountType Value, string DisplayName);
