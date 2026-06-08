using System.Globalization;

namespace Finanzuebersicht.Converters;

public class AccountIdToNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 1 || values[0] is not string accountId || string.IsNullOrEmpty(accountId))
            return string.Empty;

        if (values.Length >= 2 && values[1] is IDictionary<string, string> accountMap
            && accountMap.TryGetValue(accountId, out var name))
            return name;

        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
