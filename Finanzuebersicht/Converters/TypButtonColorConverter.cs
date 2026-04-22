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
                return paramStr == "Einnahme"
                    ? ColorResourceHelper.GetThemeColor("Einnahme", "EinnahmeDark", Color.FromArgb("#34C759"), Color.FromArgb("#30D158"))
                    : ColorResourceHelper.GetThemeColor("Ausgabe", "AusgabeDark", Color.FromArgb("#FF3B30"), Color.FromArgb("#FF453A"));

            return ColorResourceHelper.GetThemeColor("Gray400", "Gray700", Color.FromArgb("#C7C7CC"), Color.FromArgb("#636366"));
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
