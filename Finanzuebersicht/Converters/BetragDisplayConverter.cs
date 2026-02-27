using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class BetragDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Transaction t) return string.Empty;

        var vorzeichen = t.Typ == TransactionType.Einnahme ? "+" : "-";
        return $"{vorzeichen}{t.Betrag:N2} €";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
