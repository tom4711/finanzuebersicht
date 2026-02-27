using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class TransactionTypToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType typ)
            return typ == TransactionType.Einnahme ? Color.FromArgb("#34C759") : Color.FromArgb("#FF3B30");

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
