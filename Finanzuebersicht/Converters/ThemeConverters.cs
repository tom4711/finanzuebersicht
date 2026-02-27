using System.Globalization;

namespace Finanzuebersicht.Converters;

/// <summary>
/// Gibt die Hintergrundfarbe für Theme-Buttons zurück.
/// Aktiver Button = Primary, Inaktiver = Grau.
/// </summary>
public class ThemeButtonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selected && parameter is string paramStr && int.TryParse(paramStr, out var idx))
        {
            if (selected == idx)
            {
                return Application.Current?.Resources.TryGetValue("Primary", out var primary) == true
                    ? primary : Colors.Blue;
            }
        }
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#3A3A3C")
            : Color.FromArgb("#E5E5EA");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Gibt die Textfarbe für Theme-Buttons zurück.
/// Aktiver Button = Weiß, Inaktiver = Theme-abhängig.
/// </summary>
public class ThemeButtonTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selected && parameter is string paramStr && int.TryParse(paramStr, out var idx))
        {
            if (selected == idx)
                return Colors.White;
        }
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White : Color.FromArgb("#000000");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
