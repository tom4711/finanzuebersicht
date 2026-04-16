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
        var prozent = value switch
        {
            double d => d,
            decimal dec => (double)dec,
            float f => (double)f,
            int i => (double)i,
            _ => 0.0
        };
        return Math.Clamp(prozent / 100.0, 0.0, 1.0);
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

public class BudgetProgressColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? Color.FromArgb("#FF3B30")   // über Budget → rot
            : Color.FromArgb("#FF9500");  // im Budget → orange

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

/// <summary>Gibt true zurück wenn der String nicht leer ist (für IsVisible-Binding).</summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>bool → grün (true = weniger Ausgaben) oder rot (false = mehr Ausgaben).</summary>
public class TrendColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? ColorResourceHelper.GetThemeColor("Einnahme", "EinnahmeDark",
                Color.FromArgb("#34C759"), Color.FromArgb("#30D158"))
            : ColorResourceHelper.GetThemeColor("Ausgabe", "AusgabeDark",
                Color.FromArgb("#FF3B30"), Color.FromArgb("#FF453A"));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
