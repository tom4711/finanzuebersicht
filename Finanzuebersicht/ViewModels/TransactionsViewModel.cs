using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;
using Microsoft.Maui.Storage;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class TransactionsViewModel(
    DeleteTransactionUseCase deleteTransactionUseCase,
    LoadTransactionsMonthUseCase loadTransactionsMonthUseCase,
    SearchTransactionsUseCase searchTransactionsUseCase,
    INavigationService navigationService,
    ImportService importService,
    IDialogService dialogService,
    ILocalizationService localizationService,
    ICategoryRepository categoryRepository,
    ILogger<TransactionsViewModel> logger) : MonthNavigationViewModel
{
    private readonly DeleteTransactionUseCase _deleteTransactionUseCase = deleteTransactionUseCase;
    private readonly LoadTransactionsMonthUseCase _loadTransactionsMonthUseCase = loadTransactionsMonthUseCase;
    private readonly SearchTransactionsUseCase _searchTransactionsUseCase = searchTransactionsUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly ImportService _importService = importService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<TransactionsViewModel> _logger = logger;

    private CancellationTokenSource? _searchDebounce;

    // --- Monatsansicht ---

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> transaktionsGruppen = [];

    [ObservableProperty]
    private bool isLoading;

    // --- Suche & Filter ---

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private string searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterActive))]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private string? selectedKategorieId = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterActive))]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private TransactionTypeFilter selectedTypFilter = TransactionTypeFilter.Alle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterActive))]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private DateTime? vonDatum = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterActive))]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private DateTime? bisDatum = null;

    [ObservableProperty]
    private bool isFilterPanelOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    private ObservableCollection<TransactionGroup> searchErgebnisGruppen = [];

    [ObservableProperty]
    private int totalSearchCount;

    [ObservableProperty]
    private ObservableCollection<KategorieFilterItem> availableKategorien = [];

    public bool IsFilterActive =>
        SelectedKategorieId != null ||
        SelectedTypFilter != TransactionTypeFilter.Alle ||
        IsDateFilterEnabled;

    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText) || IsFilterActive;

    public bool IsMonthMode => !IsSearchActive;

    public bool HasSearchResults => SearchErgebnisGruppen.Count > 0;

    public string SearchCountText => _loc.GetString(ResourceKeys.Lbl_SuchergebnisseAnzahl, TotalSearchCount);

    // --- Picker-Hilfsfelder ---

    [ObservableProperty]
    private KategorieFilterItem? selectedKategorieFilterItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterActive))]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    [NotifyPropertyChangedFor(nameof(IsMonthMode))]
    private bool isDateFilterEnabled;

    [ObservableProperty]
    private DateTime vonDatumPicker = new DateTime(DateTime.Today.Year, 1, 1);

    [ObservableProperty]
    private DateTime bisDatumPicker = DateTime.Today;

    // Typ-Filter als Index (0=Alle, 1=Einnahmen, 2=Ausgaben) für Picker
    private int _selectedTypIndex;
    public int SelectedTypIndex
    {
        get => _selectedTypIndex;
        set
        {
            if (SetProperty(ref _selectedTypIndex, value))
            {
                SelectedTypFilter = value switch
                {
                    1 => TransactionTypeFilter.Einnahme,
                    2 => TransactionTypeFilter.Ausgabe,
                    _ => TransactionTypeFilter.Alle
                };
            }
        }
    }

    partial void OnSelectedKategorieFilterItemChanged(KategorieFilterItem? value)
    {
        SelectedKategorieId = value?.Id;
    }

    partial void OnIsDateFilterEnabledChanged(bool value)
    {
        VonDatum = value ? VonDatumPicker : null;
        BisDatum = value ? BisDatumPicker : null;
        TriggerSearchDebounced();
    }

    partial void OnVonDatumPickerChanged(DateTime value)
    {
        if (IsDateFilterEnabled) VonDatum = value;
    }

    partial void OnBisDatumPickerChanged(DateTime value)
    {
        if (IsDateFilterEnabled) BisDatum = value;
    }

    protected override async Task OnMonthChangedAsync() => await LoadTransaktionen();

    partial void OnSearchTextChanged(string value) => TriggerSearchDebounced();
    partial void OnSelectedKategorieIdChanged(string? value) => TriggerSearchDebounced();
    partial void OnSelectedTypFilterChanged(TransactionTypeFilter value) => TriggerSearchDebounced();
    partial void OnVonDatumChanged(DateTime? value) => TriggerSearchDebounced();
    partial void OnBisDatumChanged(DateTime? value) => TriggerSearchDebounced();

    private void TriggerSearchDebounced()
    {
        _searchDebounce?.Cancel();
        _searchDebounce = new CancellationTokenSource();
        var token = _searchDebounce.Token;
        Task.Run(async () =>
        {
            await Task.Delay(300, token);
            if (!token.IsCancellationRequested)
                await MainThread.InvokeOnMainThreadAsync(ExecuteSearchAsync);
        }, token);
    }

    private async Task ExecuteSearchAsync()
    {
        if (!IsSearchActive)
        {
            SearchErgebnisGruppen = [];
            TotalSearchCount = 0;
            return;
        }
        IsLoading = true;
        try
        {
            var query = new SearchTransactionsQuery(
                SearchText: SearchText.Trim(),
                KategorieId: SelectedKategorieId,
                Typ: SelectedTypFilter,
                VonDatum: VonDatum,
                BisDatum: BisDatum);
            var result = await _searchTransactionsUseCase.ExecuteAsync(query);
            SearchErgebnisGruppen = new ObservableCollection<TransactionGroup>(result.Gruppen);
            TotalSearchCount = result.TotalCount;
            Converters.KategorieIdToIconConverter.SetCache(result.IconMap);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleFilterPanel() => IsFilterPanelOpen = !IsFilterPanelOpen;

    [RelayCommand]
    private async Task ClearSearch()
    {
        _searchDebounce?.Cancel();
        SearchText = string.Empty;
        SelectedKategorieId = null;
        SelectedKategorieFilterItem = AvailableKategorien.FirstOrDefault();
        SelectedTypFilter = TransactionTypeFilter.Alle;
        SelectedTypIndex = 0;
        IsDateFilterEnabled = false;
        VonDatumPicker = new DateTime(DateTime.Today.Year, 1, 1);
        BisDatumPicker = DateTime.Today;
        VonDatum = null;
        BisDatum = null;
        IsFilterPanelOpen = false;
        SearchErgebnisGruppen = [];
        TotalSearchCount = 0;
        await LoadTransaktionen();
    }

    private async Task LoadKategorienAsync()
    {
        var kategorien = await _categoryRepository.GetCategoriesAsync();
        var items = new ObservableCollection<KategorieFilterItem>
        {
            new(null, _loc.GetString(ResourceKeys.Lbl_AlleKategorien))
        };
        foreach (var k in kategorien.OrderBy(k => k.Name))
            items.Add(new KategorieFilterItem(k.Id, $"{k.Icon} {k.Name}"));
        AvailableKategorien = items;
        SelectedKategorieFilterItem = items[0];
    }

    [RelayCommand]
    private async Task LoadTransaktionen()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var data = await _loadTransactionsMonthUseCase.ExecuteAsync(AktuellerMonat);
            TransaktionsGruppen = new ObservableCollection<TransactionGroup>(data.Gruppen);
            Converters.KategorieIdToIconConverter.SetCache(data.IconMap);

            if (AvailableKategorien.Count == 0)
                await LoadKategorienAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteTransaktion(Transaction transaktion)
    {
        try
        {
            await _deleteTransactionUseCase.ExecuteAsync(transaktion.Id);
            var gruppe = TransaktionsGruppen.FirstOrDefault(g => g.Contains(transaktion));
            if (gruppe != null)
            {
                gruppe.Remove(transaktion);
                if (gruppe.Count == 0)
                    TransaktionsGruppen.Remove(gruppe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DeleteTransaktion failed for transaction {Id}", transaktion.Id);
            try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", $"DeleteTransaktion failed for {transaktion.Id}", ex); } catch { }
            await _dialogService.ShowAlertAsync(
                _loc.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Err_Titel),
                _loc.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Err_LoeschenFehlgeschlagen, ex.Message),
                _loc.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task GoToDetail(Transaction? transaktion)
    {
        try
        {
            _logger?.LogDebug("GoToDetail called for transaction {Id}", transaktion?.Id ?? "(new)");
            try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", $"GoToDetail called for {transaktion?.Id ?? "(new)"}"); } catch { }

            var parameter = new Dictionary<string, object>();
            if (transaktion != null)
            {
                parameter["Transaction"] = transaktion;
            }

            if (_navigationService == null)
            {
                _logger?.LogError("GoToDetail: navigation service is null");
                try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", "navigation service is null"); } catch { }
                return;
            }

            await _navigationService.GoToAsync(nameof(TransactionDetailPage), parameter);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GoToDetail failed");
            try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", "GoToDetail failed", ex); } catch { }
        }
    }

    [RelayCommand]
    private async Task ImportCsv()
    {
        // Defensive checks to avoid NullReferenceExceptions when DI failed
        if (_importService == null)
        {
            var title = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportFehlgeschlagen_Title) ?? "Import fehlgeschlagen";
            var msg = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportServiceNichtVerfuegbar) ?? "ImportService nicht verfügbar.";
            var ok = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            if (_dialogService != null)
            {
                await _dialogService.ShowAlertAsync(title, msg, ok);
            }
            else
            {
                _logger?.LogError("ImportCsv: DialogService is null while handling missing ImportService");
                try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", "ImportCsv: DialogService is null while handling missing ImportService"); } catch { }
            }

            return;
        }

        try
        {
            var result = await FilePicker.PickAsync();
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            var imported = await _importService.ImportFromCsvAsync(stream);
            var count = imported?.Count() ?? 0;

            var titleDone = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportAbgeschlossen_Title) ?? "Import abgeschlossen";
            var importedMsg = string.Format(_loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportiertCount) ?? "Importiert: {0} Transaktionen", count);
            var okBtn = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            if (_dialogService != null)
            {
                await _dialogService.ShowAlertAsync(titleDone, importedMsg, okBtn);
            }
            else
            {
                _logger?.LogError("ImportCsv: DialogService is null after successful import");
                try { Finanzuebersicht.Services.FileLogger.Append("TransactionsViewModel", "ImportCsv: DialogService is null after successful import"); } catch { }
            }

            await LoadTransaktionen();

            // notify other parts of the app (dashboard/year views) that data changed
            try { App.NotifyDataChanged(); } catch { }
        }
        catch (System.Exception ex)
        {
            // Log full exception for debugging
            try
            {
                _logger?.LogError(ex, "ImportCsv failed");
            }
            catch { /* swallow logger exceptions */ }

            // Ensure we don't call a null dialog service in the catch
            var msg = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : string.Empty);
            var errTitle = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Msg_ImportFehler_Title) ?? "Fehler beim Import";
            var okError = _loc?.GetString(Finanzuebersicht.Resources.Strings.ResourceKeys.Btn_OK) ?? "OK";

            await _dialogService.ShowAlertAsync(errTitle, msg, okError);
        }
    }
}
