using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;
using Finanzuebersicht.Models;
using System.Globalization;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringId), "RecurringId")]
[QueryProperty(nameof(InstanceDate), "InstanceDate")]
public partial class RecurringInstanceShiftViewModel(
    ShiftRecurringInstanceUseCase shiftRecurringInstanceUseCase,
    INavigationService navigationService,
    IDialogService dialogService,
    ILocalizationService localizationService,
    Finanzuebersicht.Services.IClock? clock = null) : ObservableObject
{
    private readonly ShiftRecurringInstanceUseCase _shiftRecurringInstanceUseCase = shiftRecurringInstanceUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILocalizationService _loc = localizationService;
    private readonly Finanzuebersicht.Services.IClock _clock = clock ?? Finanzuebersicht.Services.SystemClock.Instance;

    [ObservableProperty]
    private string recurringId = string.Empty;

    [ObservableProperty]
    private DateTime instanceDate = Finanzuebersicht.Services.SystemClock.Instance.Today;

    [ObservableProperty]
    private DateTime newDate = Finanzuebersicht.Services.SystemClock.Instance.Today;

    [ObservableProperty]
    private string? note;

    partial void OnInstanceDateChanged(DateTime value)
    {
        NewDate = value;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(RecurringId)) return;
        try
        {
            await _shiftRecurringInstanceUseCase.ExecuteAsync(RecurringId, InstanceDate.Date, NewDate.Date, Note);
        }
        catch (Exception ex)
        {
            try { Finanzuebersicht.Services.FileLogger.Append("RecurringInstanceShiftViewModel", nameof(Save), ex); } catch { }
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await _navigationService.GoBackAsync();
    }

}
