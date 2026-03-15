using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class BetragDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Transaction t) return string.Empty;

        var ci = culture ?? CultureInfo.CurrentCulture;
        var abs = Math.Abs(t.Betrag);
        var formatted = abs.ToString("C", ci);
        var sign = t.Typ == TransactionType.Einnahme ? "+" : "-";
        return $"{sign}{formatted}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
