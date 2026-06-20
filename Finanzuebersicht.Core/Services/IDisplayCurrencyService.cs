using System.Globalization;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Display currency for monetary formatting (single currency app-wide, independent of UI language).
/// </summary>
public interface IDisplayCurrencyService
{
    IReadOnlyList<string> SupportedCodes { get; }

    string CurrencyCode { get; }

    CultureInfo FormatCulture { get; }

    int SelectedIndex { get; set; }

    event Action? Changed;
}
