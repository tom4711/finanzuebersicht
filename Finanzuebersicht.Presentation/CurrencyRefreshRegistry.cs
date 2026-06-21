using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Presentation;

/// <summary>
/// Keeps weak references to view models that display monetary amounts and refreshes them when the display currency changes.
/// </summary>
public static class CurrencyRefreshRegistry
{
    private static readonly List<WeakReference<ICurrencyRefreshViewModel>> Registered = [];

    public static void Register(ICurrencyRefreshViewModel viewModel)
    {
        Registered.RemoveAll(r => !r.TryGetTarget(out _));
        if (Registered.Any(r => r.TryGetTarget(out var existing) && ReferenceEquals(existing, viewModel)))
            return;

        Registered.Add(new WeakReference<ICurrencyRefreshViewModel>(viewModel));
    }

    public static void RefreshAll()
    {
        Registered.RemoveAll(r => !r.TryGetTarget(out _));
        foreach (var reference in Registered.ToList())
        {
            if (reference.TryGetTarget(out var viewModel))
                viewModel.RefreshCurrencyDisplay();
        }
    }
}
