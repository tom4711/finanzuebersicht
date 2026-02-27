using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class TypButtonColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType typ && parameter is string paramStr)
        {
            var isMatch = paramStr == "Einnahme"
                ? typ == TransactionType.Einnahme
                : typ == TransactionType.Ausgabe;

            if (isMatch)
                return paramStr == "Einnahme" ? Color.FromArgb("#34C759") : Color.FromArgb("#FF3B30");

            return Color.FromArgb("#C7C7CC");
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
