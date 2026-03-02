using System.Globalization;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Converters;

/// <summary>
/// Gibt den angezeigten Namen einer Kategorie zurück.
/// Systemkategorien (SystemKey != null) werden über LocalizationResourceManager übersetzt,
/// nutzerdefinierte Kategorien geben ihren Name-Wert unverändert zurück.
/// </summary>
public class KategorieNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Category kategorie) return value;

        if (!string.IsNullOrEmpty(kategorie.SystemKey))
        {
            var translated = LocalizationResourceManager.Current[kategorie.SystemKey];
            if (!string.IsNullOrEmpty(translated))
                return translated;
        }

        return kategorie.Name;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
