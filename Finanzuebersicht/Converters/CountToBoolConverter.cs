using System.Globalization;
using System.Linq;
using System.Collections;

namespace Finanzuebersicht.Converters;

public class CountToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 0;

        if (value is IEnumerable enumerable)
        {
            foreach (var _ in enumerable)
                return true;
            return false;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
