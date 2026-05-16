using System.Globalization;

namespace Finanzuebersicht.Converters;

/// <summary>
/// Converts a KategorieId (values[0]) to its icon emoji using an IconMap dictionary (values[1]).
/// Using IMultiValueConverter avoids a static shared cache.
/// </summary>
public class KategorieIdToIconConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 1 || values[0] is not string kategorieId || string.IsNullOrEmpty(kategorieId))
            return "📁";

        if (values.Length >= 2 && values[1] is IDictionary<string, string> iconMap
            && iconMap.TryGetValue(kategorieId, out var icon))
            return icon;

        return "📁";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
