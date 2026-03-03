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
                return ColorResourceHelper.GetColor("Primary", Colors.Blue);
            }
        }

        return ColorResourceHelper.GetThemeColor(
            "Gray200",
            "Gray900",
            Color.FromArgb("#E5E5EA"),
            Color.FromArgb("#3A3A3C"));
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
                return ColorResourceHelper.GetColor("White", Colors.White);
        }

        return ColorResourceHelper.GetThemeColor(
            "Black",
            "White",
            Color.FromArgb("#000000"),
            Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
