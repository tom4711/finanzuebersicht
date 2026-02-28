using System.Globalization;

namespace Finanzuebersicht.Converters;

public class KategorieIdToIconConverter : IValueConverter
{
    // Static cache to avoid repeated lookups
    private static Dictionary<string, string>? _iconCache;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string kategorieId || string.IsNullOrEmpty(kategorieId))
            return "📁";

        // Lazy load cache on first use
        if (_iconCache == null)
        {
            _iconCache = new();
            var dataService = ServiceRegistry.DataService;
            if (dataService != null)
            {
                try
                {
                    var categories = dataService.GetCategoriesAsync().Result;
                    foreach (var cat in categories)
                    {
                        _iconCache[cat.Id] = cat.Icon ?? "📁";
                    }
                }
                catch
                {
                    // Service not available, use fallback
                }
            }
        }

        // Return from cache if available
        if (_iconCache.TryGetValue(kategorieId, out var icon))
            return icon;

        return "📁";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
