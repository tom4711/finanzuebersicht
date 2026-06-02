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
    ILogger<AccountDetailViewModel>? logger = null) : ObservableObject, IApplyQueryAttributes
{
    private readonly SaveAccountDetailUseCase _saveAccountDetailUseCase = saveAccountDetailUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ILogger<AccountDetailViewModel>? _logger = logger;
    private Account? _existingAccount;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private AccountType type = AccountType.Girokonto;

    public string PageTitle => _existingAccount == null ? "Konto hinzufügen" : "Konto bearbeiten";

    public List<AccountType> VerfuegbareTypen { get; } = Enum.GetValues<AccountType>().ToList();

    public Account? Account
    {
        set
        {
            if (value != null)
            {
                _existingAccount = value;
                Name = value.Name;
                Type = value.Type;
                OnPropertyChanged(nameof(PageTitle));
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

        await _saveAccountDetailUseCase.ExecuteAsync(_existingAccount, Name, Type);
        await _navigationService.GoBackAsync();
    }
}
