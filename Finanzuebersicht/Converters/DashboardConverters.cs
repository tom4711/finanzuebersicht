using System.Globalization;

namespace Finanzuebersicht.Converters;

public class BilanzColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal bilanz)
            return bilanz >= 0 ? Color.FromArgb("#34C759") : Color.FromArgb("#FF3B30");

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ProzentToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double prozent)
            return Math.Max(prozent / 100.0, 0.02); // Minimum 2% sichtbar

        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DecimalCurrencyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal betrag)
            return $"{betrag:N2} €";

        return "0,00 €";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BilanzDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal bilanz)
        {
            var vorzeichen = bilanz >= 0 ? "+" : "";
            return $"{vorzeichen}{bilanz:N2} €";
        }

        return "0,00 €";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
