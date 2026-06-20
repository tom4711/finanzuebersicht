using System.Globalization;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;
using Microsoft.Maui.Controls;

namespace Finanzuebersicht.Converters;

public class TransactionTypeToLocalizedStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TransactionType transactionType)
            return string.Empty;

        return LocalizationResourceManager.Current[EnumResourceKeys.GetTransactionType(transactionType)];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
