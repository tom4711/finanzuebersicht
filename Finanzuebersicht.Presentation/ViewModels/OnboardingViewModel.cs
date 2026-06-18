using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Presentation.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

public partial class OnboardingViewModel(
    IOnboardingCoordinator onboardingCoordinator,
    INavigationService navigationService,
    ILocalizationService localizationService,
    LoadAccountsUseCase loadAccountsUseCase) : ObservableObject
{
    private readonly IOnboardingCoordinator _onboardingCoordinator = onboardingCoordinator;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly LoadAccountsUseCase _loadAccountsUseCase = loadAccountsUseCase;

    [ObservableProperty]
    private int currentStep;

    public int TotalSteps => 4;

    public string StepIndicator => $"{CurrentStep + 1} / {TotalSteps}";

    public bool ShowAccountActions => CurrentStep == 1;
    public bool ShowTransactionActions => CurrentStep == 2;
    public bool ShowBackupActions => CurrentStep == 3;

    public string StepTitle => CurrentStep switch
    {
        0 => _loc.GetString(ResourceKeys.Onboarding_TitleWelcome),
        1 => _loc.GetString(ResourceKeys.Onboarding_TitleAccount),
        2 => _loc.GetString(ResourceKeys.Onboarding_TitleFirstTransaction),
        _ => _loc.GetString(ResourceKeys.Onboarding_TitleBackup)
    };

    public string StepDescription => CurrentStep switch
    {
        0 => _loc.GetString(ResourceKeys.Onboarding_DescWelcome),
        1 => _loc.GetString(ResourceKeys.Onboarding_DescAccount),
        2 => _loc.GetString(ResourceKeys.Onboarding_DescFirstTransaction),
        _ => _loc.GetString(ResourceKeys.Onboarding_DescBackup)
    };

    public bool IsLastStep => CurrentStep >= TotalSteps - 1;
    public bool IsFirstStep => CurrentStep == 0;

    public string PrimaryButtonText => IsLastStep
        ? _loc.GetString(ResourceKeys.Btn_Fertig)
        : _loc.GetString(ResourceKeys.Btn_Weiter);

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(StepTitle));
        OnPropertyChanged(nameof(StepDescription));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(PrimaryButtonText));
        OnPropertyChanged(nameof(StepIndicator));
        OnPropertyChanged(nameof(ShowAccountActions));
        OnPropertyChanged(nameof(ShowTransactionActions));
        OnPropertyChanged(nameof(ShowBackupActions));
    }

    [RelayCommand]
    private void Next()
    {
        if (IsLastStep)
        {
            Complete();
            return;
        }

        CurrentStep++;
    }

    [RelayCommand]
    private void Back()
    {
        if (CurrentStep > 0)
            CurrentStep--;
    }

    [RelayCommand]
    private void Skip()
        => Complete();

    [RelayCommand]
    private async Task OpenDefaultAccount()
    {
        var accounts = await _loadAccountsUseCase.ExecuteAsync();
        var account = accounts.FirstOrDefault(a => !a.IsArchived)
            ?? accounts.FirstOrDefault();
        if (account is null) return;

        await _navigationService.GoToAsync(Routes.AccountDetail, new Dictionary<string, object>
        {
            ["Account"] = account
        });
    }

    [RelayCommand]
    private async Task OpenNewTransaction()
    {
        CompleteNavigationPending();
        await _navigationService.GoToAsync("//TransactionsPage");
        await _navigationService.GoToAsync(Routes.TransactionDetail);
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        CompleteNavigationPending();
        await _navigationService.GoToAsync(Routes.Settings);
    }

    private void CompleteNavigationPending()
    {
        _onboardingCoordinator.MarkCompleted();
        _ = _navigationService.GoBackAsync();
    }

    private void Complete()
    {
        _onboardingCoordinator.MarkCompleted();
        _ = _navigationService.GoBackAsync();
    }
}
