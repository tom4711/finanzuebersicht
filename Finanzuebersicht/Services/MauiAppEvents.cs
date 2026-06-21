namespace Finanzuebersicht.Services;

/// <summary>
/// Forwards data-changed notifications to <see cref="App.DataChanged"/>
/// without exposing the MAUI App class to ViewModels.
/// </summary>
public class MauiAppEvents : IAppEvents
{
    public MauiAppEvents() => App.CurrencyChanged += () => CurrencyChanged?.Invoke();

    public event Action? CurrencyChanged;

    public void NotifyDataChanged() => App.NotifyDataChanged();
}
