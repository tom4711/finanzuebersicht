namespace Finanzuebersicht.Services;

/// <summary>
/// Abstracts app-wide event notification so ViewModels don't depend on the
/// MAUI App class directly.
/// </summary>
public interface IAppEvents
{
    void NotifyDataChanged();
}
