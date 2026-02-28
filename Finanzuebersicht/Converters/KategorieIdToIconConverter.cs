using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class KategorieIdToIconConverter : IValueConverter
{
    // Static cache to avoid repeated lookups
    private static Dictionary<string, string> _iconCache = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string kategorieId || string.IsNullOrEmpty(kategorieId))
            return "📁";

        // Return from cache if available
        if (_iconCache.TryGetValue(kategorieId, out var icon))
            return icon;

        return "📁";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    // Static method to populate cache from TransactionsViewModel or Page
    public static void SetCache(Dictionary<string, string> kategorieIdToIconMap)
    {
        _iconCache = kategorieIdToIconMap;
    }
}
