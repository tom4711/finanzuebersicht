using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringTransaction), "RecurringTransaction")]
public partial class RecurringTransactionDetailViewModel(
    SaveRecurringTransactionDetailUseCase saveRecurringTransactionDetailUseCase,
    LoadRecurringTransactionDetailDataUseCase loadRecurringTransactionDetailDataUseCase,
    AddRecurringExceptionUseCase addRecurringExceptionUseCase,
    RemoveRecurringExceptionUseCase removeRecurringExceptionUseCase,
    ITransactionValidationService validationService,
    INavigationService navigationService) : ObservableObject
{
    private readonly SaveRecurringTransactionDetailUseCase _saveRecurringTransactionDetailUseCase = saveRecurringTransactionDetailUseCase;
    private readonly LoadRecurringTransactionDetailDataUseCase _loadRecurringTransactionDetailDataUseCase = loadRecurringTransactionDetailDataUseCase;
    private readonly AddRecurringExceptionUseCase _addRecurringExceptionUseCase = addRecurringExceptionUseCase;
    private readonly RemoveRecurringExceptionUseCase _removeRecurringExceptionUseCase = removeRecurringExceptionUseCase;
    private readonly ITransactionValidationService _validationService = validationService;
    private RecurringTransaction? _existing;
    private readonly INavigationService _navigationService = navigationService;

    [ObservableProperty]
    private string betragText = string.Empty;

    [ObservableProperty]
    private string titel = string.Empty;

    [ObservableProperty]
    private Category? selectedKategorie;

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

    [ObservableProperty]
    private DateTime startdatum = DateTime.Today;

    [ObservableProperty]
    private DateTime? enddatum;

    [ObservableProperty]
    private bool hatEnddatum;

    [ObservableProperty]
    private DateTime enddatumWert = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private bool aktiv = true;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    private RecurrenceInterval interval = RecurrenceInterval.Monthly;

    [ObservableProperty]
    private int intervalFactor = 1;

    [ObservableProperty]
    private int reminderDaysBefore = 0;

    [ObservableProperty]
    private ObservableCollection<RecurringException> exceptions = [];

    public List<string> IntervalOptions { get; } = new List<string> { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" };

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
                ReminderDaysBefore = value.ReminderDaysBefore;
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
                out _))
            return;

        // logging removed: use centralized logging if needed
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

        await _navigationService.GoToAsync("RecurringInstanceShiftPage", parameters);
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
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Kategorie: {ex.Message}");
        }
    }
}
