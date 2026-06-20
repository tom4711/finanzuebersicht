namespace Finanzuebersicht.Converters;

internal static class ColorResourceHelper
{
    public static Color GetColor(string resourceKey, Color fallback)
    {
        if (global::Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true)
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
        var isDarkTheme = IsDarkThemeActive();
        return isDarkTheme
            ? GetColor(darkResourceKey, darkFallback)
            : GetColor(lightResourceKey, lightFallback);
    }

    private static bool IsDarkThemeActive()
    {
        var app = global::Microsoft.Maui.Controls.Application.Current;
        if (app is null)
            return false;

        return app.RequestedTheme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            _ => app.PlatformAppTheme == AppTheme.Dark
        };
    }
}