namespace Finanzuebersicht.Converters;

internal static class ColorResourceHelper
{
    public static Color GetColor(string resourceKey, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true)
        {
            if (resource is Color color)
                return color;

            if (resource is SolidColorBrush brush)
                return brush.Color;
        }

        return fallback;
    }

    public static Color GetThemeColor(string lightResourceKey, string darkResourceKey, Color lightFallback, Color darkFallback)
    {
        var isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
        return isDarkTheme
            ? GetColor(darkResourceKey, darkFallback)
            : GetColor(lightResourceKey, lightFallback);
    }
}