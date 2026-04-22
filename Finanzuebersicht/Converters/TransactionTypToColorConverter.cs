using System.Globalization;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class TransactionTypToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType typ)
            return typ == TransactionType.Einnahme
                ? ColorResourceHelper.GetThemeColor("Einnahme", "EinnahmeDark", Color.FromArgb("#34C759"), Color.FromArgb("#30D158"))
                : ColorResourceHelper.GetThemeColor("Ausgabe", "AusgabeDark", Color.FromArgb("#FF3B30"), Color.FromArgb("#FF453A"));

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
