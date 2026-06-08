using System.Globalization;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Converters;

public class AccountArchiveActionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isArchived = value switch
        {
            bool b => b,
            Finanzuebersicht.Models.Account account => account.IsArchived,
            _ => false
        };

        return isArchived
            ? LocalizationResourceManager.Current["Btn_Aktivieren"]
            : LocalizationResourceManager.Current["Btn_Archivieren"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
