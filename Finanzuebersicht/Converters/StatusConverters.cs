using System.Globalization;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Converters;

public class VorzeichenConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TransactionType.Einnahme ? "+" : "-";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class AktivTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? LocalizationResourceManager.Current[ResourceKeys.Status_Aktiv] 
                         : LocalizationResourceManager.Current[ResourceKeys.Status_Inaktiv];

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
