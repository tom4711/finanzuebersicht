using System.Globalization;
using Finanzuebersicht.Models;
using Microsoft.Maui.Controls;

namespace Finanzuebersicht.Converters;

public class AccountTypeToLocalizedStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AccountType accountType)
            return string.Empty;

        return accountType switch
        {
            AccountType.Girokonto => LocalizationResourceManager.Current["AccountType_Girokonto"],
            AccountType.Tagesgeld => LocalizationResourceManager.Current["AccountType_Tagesgeld"],
            AccountType.Kreditkarte => LocalizationResourceManager.Current["AccountType_Kreditkarte"],
            AccountType.Bargeld => LocalizationResourceManager.Current["AccountType_Bargeld"],
            AccountType.Depot => LocalizationResourceManager.Current["AccountType_Depot"],
            AccountType.Sonstiges => LocalizationResourceManager.Current["AccountType_Sonstiges"],
            _ => accountType.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
