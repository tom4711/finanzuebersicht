using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Services;
using Finanzuebersicht.Models;
using System.Globalization;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(RecurringId), "RecurringId")]
[QueryProperty(nameof(InstanceDate), "InstanceDate")]
public partial class RecurringInstanceShiftViewModel(
    ShiftRecurringInstanceUseCase shiftRecurringInstanceUseCase,
    INavigationService navigationService,
    Finanzuebersicht.Core.Services.IClock? clock = null) : ObservableObject
{
    private readonly ShiftRecurringInstanceUseCase _shiftRecurringInstanceUseCase = shiftRecurringInstanceUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly Finanzuebersicht.Core.Services.IClock _clock = clock ?? Finanzuebersicht.Core.Services.SystemClock.Instance;

    [ObservableProperty]
    private string recurringId = string.Empty;

    [ObservableProperty]
    private DateTime instanceDate = Finanzuebersicht.Core.Services.SystemClock.Instance.Today;

    [ObservableProperty]
    private DateTime newDate = Finanzuebersicht.Core.Services.SystemClock.Instance.Today;

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
        await _shiftRecurringInstanceUseCase.ExecuteAsync(RecurringId, InstanceDate.Date, NewDate.Date, Note);
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await _navigationService.GoBackAsync();
    }

}
