namespace Finanzuebersicht.ViewModels;

/// <summary>
/// ViewModels that display monetary amounts should refresh bindings when the display currency changes.
/// </summary>
public interface ICurrencyRefreshViewModel
{
    void RefreshCurrencyDisplay();
}
