using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringTransaction), "RecurringTransaction")]
public partial class RecurringTransactionDetailViewModel(
    SaveRecurringTransactionDetailUseCase saveRecurringTransactionDetailUseCase,
    LoadRecurringTransactionDetailDataUseCase loadRecurringTransactionDetailDataUseCase,
    AddRecurringExceptionUseCase addRecurringExceptionUseCase,
    RemoveRecurringExceptionUseCase removeRecurringExceptionUseCase,
    ITransactionValidationService validationService,
    INavigationService navigationService,
    IDialogService dialogService,
    ILocalizationService localizationService,
    ILogger<RecurringTransactionDetailViewModel>? logger = null,
    Finanzuebersicht.Services.IClock? clock = null) : ObservableObject
{
    private readonly SaveRecurringTransactionDetailUseCase _saveRecurringTransactionDetailUseCase = saveRecurringTransactionDetailUseCase;
    private readonly LoadRecurringTransactionDetailDataUseCase _loadRecurringTransactionDetailDataUseCase = loadRecurringTransactionDetailDataUseCase;
    private readonly AddRecurringExceptionUseCase _addRecurringExceptionUseCase = addRecurringExceptionUseCase;
    private readonly RemoveRecurringExceptionUseCase _removeRecurringExceptionUseCase = removeRecurringExceptionUseCase;
    private readonly ITransactionValidationService _validationService = validationService;
    private RecurringTransaction? _existing;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly ILogger<RecurringTransactionDetailViewModel>? _logger = logger;
    private readonly Finanzuebersicht.Services.IClock _clock = clock ?? Finanzuebersicht.Services.SystemClock.Instance;

    [ObservableProperty]
    private string betragText = string.Empty;

    [ObservableProperty]
    private string titel = string.Empty;

    [ObservableProperty]
    private Category? selectedKategorie;

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

    [ObservableProperty]
    private DateTime startdatum = Finanzuebersicht.Services.SystemClock.Instance.Today;

    [ObservableProperty]
    private DateTime? enddatum;

    [ObservableProperty]
    private bool hatEnddatum;

    [ObservableProperty]
    private DateTime enddatumWert = Finanzuebersicht.Services.SystemClock.Instance.Today.AddYears(1);

    [ObservableProperty]
    private bool aktiv = true;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    private RecurrenceInterval interval = RecurrenceInterval.Monthly;

    // string-backed entries to avoid binding conversion errors; numeric backing kept for logic
    [ObservableProperty]
    private string intervalFactorText = "1";

    [ObservableProperty]
    private int intervalFactor = 1;

    [ObservableProperty]
    private string reminderDaysBeforeText = "0";

    [ObservableProperty]
    private int reminderDaysBefore = 0;

    [ObservableProperty]
    private ObservableCollection<RecurringException> exceptions = [];

    // Prefer binding the Picker directly to enum values to avoid string localization issues
    public List<RecurrenceInterval> IntervalValues { get; } = Enum.GetValues<RecurrenceInterval>().Cast<RecurrenceInterval>().ToList();

    public RecurringTransaction? RecurringTransaction
    {
        set
        {
            if (value != null)
            {
                _existing = value;
                BetragText = value.Betrag.ToString("F2", System.Globalization.CultureInfo.CurrentCulture);
                Titel = value.Titel;
                Typ = value.Typ;
                Startdatum = value.Startdatum;
                Aktiv = value.Aktiv;

                if (value.Enddatum.HasValue)
                {
                    HatEnddatum = true;
                    EnddatumWert = value.Enddatum.Value;
                }

                _ = SetKategorieAsync(value.KategorieId);
                Interval = value.Interval;
                IntervalFactor = value.IntervalFactor;
                IntervalFactorText = value.IntervalFactor.ToString();
                ReminderDaysBefore = value.ReminderDaysBefore;
                ReminderDaysBeforeText = value.ReminderDaysBefore.ToString();
                Exceptions = new ObservableCollection<RecurringException>(value.Exceptions ?? new List<RecurringException>());
            }
        }
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        var currentId = SelectedKategorie?.Id ?? _existing?.KategorieId;
        var data = await _loadRecurringTransactionDetailDataUseCase.ExecuteAsync(currentId);
        Kategorien = new ObservableCollection<Category>(data.Kategorien);
        SelectedKategorie = data.SelectedKategorie;
    }

    [RelayCommand]
    private void SetTyp(string typName)
    {
        Typ = typName == "Einnahme" ? TransactionType.Einnahme : TransactionType.Ausgabe;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!_validationService.TryValidate(
                BetragText,
                Titel,
                SelectedKategorie != null,
                System.Globalization.CultureInfo.CurrentCulture,
                out var betrag,
                out var error))
        {
            var message = error switch
            {
                TransactionInputError.InvalidAmountFormat => _loc.GetString(ResourceKeys.Err_UngueltigerBetrag),
                TransactionInputError.AmountMustBePositive => _loc.GetString(ResourceKeys.Err_BetragGroesserNull),
                TransactionInputError.TitleRequired => _loc.GetString(ResourceKeys.Err_TitelErforderlich),
                TransactionInputError.CategoryRequired => _loc.GetString(ResourceKeys.Err_KategorieErforderlich),
                _ => _loc.GetString(ResourceKeys.Err_UngueltigerBetrag)
            };
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                message,
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        // parse numeric text fields (Entry bindings)
        if (!int.TryParse(IntervalFactorText, out var parsedFactor) || parsedFactor <= 0)
        {
            // Ungültige oder zu kleine Werte auf 1 clampen und im UI anzeigen
            parsedFactor = 1;
            IntervalFactorText = "1";
        }

        if (!int.TryParse(ReminderDaysBeforeText, out var parsedReminder) || parsedReminder < 0)
        {
            // Ungültige oder negative Werte auf 0 clampen und im UI anzeigen
            parsedReminder = 0;
            ReminderDaysBeforeText = "0";
        }

        IntervalFactor = parsedFactor;
        ReminderDaysBefore = parsedReminder;

        await _saveRecurringTransactionDetailUseCase.ExecuteAsync(
            _existing,
            betrag,
            Titel,
            SelectedKategorie!.Id,
            Typ,
            Startdatum,
            HatEnddatum ? EnddatumWert : null,
            Aktiv,
            Interval,
            IntervalFactor,
            ReminderDaysBefore,
            Exceptions.ToList());
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task AddSkipNextInstance()
    {
        if (_existing == null) return;

        DateTime next;
        if (_existing.LetzteAusfuehrung.HasValue)
        {
            next = GetNextInstanceLocal(_existing, _existing.LetzteAusfuehrung.Value, IntervalFactor);
        }
        else
        {
            // if no last execution, the next instance is the start date itself
            next = _existing.Startdatum.Date;
        }

        var ex = new RecurringException { Id = Guid.NewGuid().ToString(), InstanceDate = next.Date, Type = RecurringExceptionType.Skip };
        await _addRecurringExceptionUseCase.ExecuteAsync(_existing.Id, ex);
        Exceptions.Add(ex);
    }

    [RelayCommand]
    private async Task GoToShift()
    {
        if (_existing == null) return;

        DateTime next;
        if (_existing.LetzteAusfuehrung.HasValue)
        {
            next = GetNextInstanceLocal(_existing, _existing.LetzteAusfuehrung.Value, IntervalFactor);
        }
        else
        {
            next = _existing.Startdatum.Date;
        }

        var parameters = new Dictionary<string, object>
        {
            ["RecurringId"] = _existing.Id,
            ["InstanceDate"] = next.Date
        };

        await _navigationService.GoToAsync(nameof(RecurringInstanceShiftPage), parameters);
    }

    private static DateTime GetNextInstanceLocal(RecurringTransaction recurring, DateTime fromDate, int intervalFactor)
    {
        var factor = Math.Max(1, intervalFactor);
        return recurring.Interval switch
        {
            RecurrenceInterval.Weekly => fromDate.Date.AddDays(7L * factor),
            RecurrenceInterval.Monthly => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
            RecurrenceInterval.Quarterly => AddMonthsPreserveDay(fromDate.Date, 3 * factor),
            RecurrenceInterval.Yearly => AddMonthsPreserveDay(fromDate.Date, 12 * factor),
            RecurrenceInterval.Daily => fromDate.Date.AddDays(1 * factor),
            _ => AddMonthsPreserveDay(fromDate.Date, 1 * factor),
        };
    }

    private static DateTime AddMonthsPreserveDay(DateTime date, int months)
    {
        var target = date.AddMonths(months);
        var day = date.Day;
        var daysInTarget = DateTime.DaysInMonth(target.Year, target.Month);
        if (day > daysInTarget)
            day = daysInTarget;
        return new DateTime(target.Year, target.Month, day);
    }

    [RelayCommand]
    private async Task RemoveException(RecurringException ex)
    {
        if (_existing == null || ex == null) return;
        await _removeRecurringExceptionUseCase.ExecuteAsync(_existing.Id, ex.Id);
        Exceptions.Remove(ex);
    }

    private async Task SetKategorieAsync(string kategorieId)
    {
        try
        {
            if (Kategorien.Count == 0)
                await LoadKategorien();

            SelectedKategorie = Kategorien.FirstOrDefault(k => k.Id == kategorieId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Fehler beim Laden der Kategorie");
        }
    }
}
