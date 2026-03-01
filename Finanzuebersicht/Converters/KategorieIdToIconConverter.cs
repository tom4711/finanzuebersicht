using System.Globalization;

namespace Finanzuebersicht.Converters;

public class KategorieIdToIconConverter : IValueConverter
{
    // Static cache - populated when app starts
    private static Dictionary<string, string> _iconCache = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string kategorieId || string.IsNullOrEmpty(kategorieId))
            return "📁";

        // Return from cache (already loaded by TransactionsPage.xaml.cs)
        if (_iconCache.TryGetValue(kategorieId, out var icon))
            return icon;

        return "📁";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    // Static method to populate cache - called from TransactionsPage.xaml.cs
    public static void SetCache(Dictionary<string, string> kategorieIdToIconMap)
    {
        _iconCache = kategorieIdToIconMap;
    }
}
