using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Presentation.Services;
using Finanzuebersicht.Resources.Strings;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class CategoriesViewModel(
    DeleteCategoryUseCase deleteCategoryUseCase,
    LoadCategoriesUseCase loadCategoriesUseCase,
    LoadAccountsUseCase loadAccountsUseCase,
    GetAccountBalancesUseCase getAccountBalancesUseCase,
    ToggleAccountArchiveUseCase toggleAccountArchiveUseCase,
    DeleteAccountUseCase deleteAccountUseCase,
    ILocalizationService localizationService,
    INavigationService navigationService,
    IDialogService dialogService,
    IFeedbackService feedbackService,
    IAppEvents appEvents,
    ILogger<CategoriesViewModel>? logger = null) : ObservableObject, IAutoLoadViewModel
{
    private readonly DeleteCategoryUseCase _deleteCategoryUseCase = deleteCategoryUseCase;
    private readonly LoadCategoriesUseCase _loadCategoriesUseCase = loadCategoriesUseCase;
    private readonly LoadAccountsUseCase _loadAccountsUseCase = loadAccountsUseCase;
    private readonly GetAccountBalancesUseCase _getAccountBalancesUseCase = getAccountBalancesUseCase;
    private readonly ToggleAccountArchiveUseCase _toggleAccountArchiveUseCase = toggleAccountArchiveUseCase;
    private readonly DeleteAccountUseCase _deleteAccountUseCase = deleteAccountUseCase;
    private readonly ILocalizationService _loc = localizationService;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IFeedbackService _feedbackService = feedbackService;
    private readonly IAppEvents _appEvents = appEvents;
    private readonly ILogger<CategoriesViewModel>? _logger = logger;

    public System.Windows.Input.ICommand AutoLoadCommand => LoadKategorienCommand;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowGesamtSaldoHeader))]
    private ObservableCollection<AccountListItem> konten = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowGesamtSaldoHeader))]
    private decimal gesamtSaldoAktiv;

    public bool ShowGesamtSaldoHeader => Konten.Any(k => !k.IsArchived);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsKategorienVisible))]
    [NotifyPropertyChangedFor(nameof(IsKontenVisible))]
    private int selectedSectionIndex;

    public bool IsKategorienVisible => SelectedSectionIndex == 0;
    public bool IsKontenVisible => SelectedSectionIndex == 1;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private void ShowKategorien() => SelectedSectionIndex = 0;

    [RelayCommand]
    private void ShowKonten() => SelectedSectionIndex = 1;

    [RelayCommand]
    private async Task LoadKategorien()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _loadCategoriesUseCase.ExecuteAsync();
            Kategorien = new ObservableCollection<Category>(liste);
            var accounts = await _loadAccountsUseCase.ExecuteAsync();
            var balances = await _getAccountBalancesUseCase.ExecuteAsync();
            var balanceById = balances.ToDictionary(b => b.AccountId);
            Konten = new ObservableCollection<AccountListItem>(
                accounts
                    .OrderBy(a => a.IsArchived)
                    .ThenBy(a => a.Name)
                    .Select(a =>
                    {
                        balanceById.TryGetValue(a.Id, out var summary);
                        return new AccountListItem(a, summary)
                        {
                            BalanceBreakdownText = summary is { OpeningBalance: not 0 }
                                ? _loc.GetString(
                                    ResourceKeys.Fmt_KontoSaldoAufschluesselung,
                                    summary.OpeningBalance.ToString("C", CultureInfo.CurrentCulture),
                                    summary.TransactionBalance.ToString("C", CultureInfo.CurrentCulture))
                                : null
                        };
                    }));
            GesamtSaldoAktiv = balances.Where(b => !b.IsArchived).Sum(b => b.Saldo);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CategoriesViewModel: {Context}", nameof(LoadKategorien));
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

    [RelayCommand]
    private async Task DeleteKategorie(Category kategorie)
    {
        var confirm = await _dialogService.ShowConfirmationAsync(
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschen),
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschenFrage, kategorie.Name),
            _loc.GetString(ResourceKeys.Btn_Ja), _loc.GetString(ResourceKeys.Btn_Nein));
        if (!confirm) return;

        try
        {
            await _deleteCategoryUseCase.ExecuteAsync(kategorie.Id);
            Kategorien.Remove(kategorie);
            _appEvents.NotifyDataChanged();
            await _feedbackService.ShowSnackbarAsync(_loc.GetString(ResourceKeys.Msg_Geloescht));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CategoriesViewModel: {Context}", nameof(DeleteKategorie));
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_LoeschenFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task DeleteKonto(AccountListItem konto)
    {
        if (!konto.CanDelete) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Konto löschen",
            $"\"{konto.Name}\" wirklich löschen?",
            _loc.GetString(ResourceKeys.Btn_Ja), _loc.GetString(ResourceKeys.Btn_Nein));
        if (!confirm) return;

        try
        {
            await _deleteAccountUseCase.ExecuteAsync(konto.Account.Id);
            Konten.Remove(konto);
            _appEvents.NotifyDataChanged();
            await _feedbackService.ShowSnackbarAsync(_loc.GetString(ResourceKeys.Msg_Geloescht));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CategoriesViewModel: {Context}", nameof(DeleteKonto));
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_LoeschenFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task ToggleKontoArchivierung(AccountListItem konto)
    {
        if (!konto.CanArchive) return;

        var setArchived = !konto.IsArchived;
        var confirmTitle = setArchived ? "Konto archivieren" : "Konto reaktivieren";
        var confirmBody = setArchived
            ? $"\"{konto.Name}\" archivieren? Konto steht dann nicht mehr für neue Buchungen zur Auswahl."
            : $"\"{konto.Name}\" wieder aktivieren?";
        var confirm = await _dialogService.ShowConfirmationAsync(
            confirmTitle,
            confirmBody,
            _loc.GetString(ResourceKeys.Btn_Ja),
            _loc.GetString(ResourceKeys.Btn_Nein));
        if (!confirm) return;

        try
        {
            await _toggleAccountArchiveUseCase.ExecuteAsync(konto.Account, setArchived);
            await LoadKategorien();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CategoriesViewModel: {Context}", nameof(ToggleKontoArchivierung));
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task GoToDetail(object? item = null)
    {
        if (item is AccountListItem kontoItem)
        {
            var kontoParameter = new Dictionary<string, object>
            {
                ["Account"] = kontoItem.Account
            };
            await _navigationService.GoToAsync(Routes.AccountDetail, kontoParameter);
            return;
        }

        if (item == null && IsKontenVisible)
        {
            await _navigationService.GoToAsync(Routes.AccountDetail);
            return;
        }

        var parameter = new Dictionary<string, object>();
        if (item is Category kategorie)
            parameter["Category"] = kategorie;

        await _navigationService.GoToAsync(Routes.CategoryDetail, parameter);
    }
}
