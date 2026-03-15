using System.Globalization;

namespace Finanzuebersicht.Converters;

public class BilanzColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal bilanz)
            return bilanz >= 0
                ? ColorResourceHelper.GetColor("Einnahme", Color.FromArgb("#34C759"))
                : ColorResourceHelper.GetColor("Ausgabe", Color.FromArgb("#FF3B30"));

        return ColorResourceHelper.GetColor("Gray600", Colors.Gray);
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
        {
            var ci = culture ?? CultureInfo.CurrentCulture;
            return betrag.ToString("C", ci);
        }

        return 0m.ToString("C", culture ?? CultureInfo.CurrentCulture);
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
            var ci = culture ?? CultureInfo.CurrentCulture;
            var abs = Math.Abs(bilanz);
            var sign = bilanz >= 0 ? "+" : "-";
            return $"{sign}{abs.ToString("C", ci)}";
        }

        return 0m.ToString("C", culture ?? CultureInfo.CurrentCulture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ToggleActiveColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? ColorResourceHelper.GetColor("Primary", Color.FromArgb("#007AFF"))
            : Colors.Transparent;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ToggleActiveTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? ColorResourceHelper.GetColor("White", Colors.White)
            : ColorResourceHelper.GetColor("Primary", Color.FromArgb("#007AFF"));

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
