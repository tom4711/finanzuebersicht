using System.Globalization;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Fixed EUR formatting for monetary amounts regardless of UI language.
/// </summary>
public static class CurrencyCulture
{
    public static CultureInfo Instance { get; } = CultureInfo.GetCultureInfo("de-DE");
}
