using System.Globalization;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Monetary formatting culture derived from the configured display currency.
/// </summary>
public static class CurrencyCulture
{
    private static IDisplayCurrencyService? _service;

    public static CultureInfo Instance =>
        _service?.FormatCulture ?? CultureInfo.GetCultureInfo("de-DE");

    public static void Initialize(IDisplayCurrencyService service) => _service = service;
}
