using Finanzuebersicht.Models;
using System.Globalization;

namespace Finanzuebersicht.Converters;

public class ImportPreviewStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ImportPreviewRowStatus status)
            return ColorResourceHelper.GetThemeColor("Gray600", "Gray600", Color.FromArgb("#8E8E93"), Color.FromArgb("#8E8E93"));

        return status switch
        {
            ImportPreviewRowStatus.Ready => ColorResourceHelper.GetThemeColor(
                "Einnahme", "EinnahmeDark", Color.FromArgb("#34C759"), Color.FromArgb("#30D158")),
            ImportPreviewRowStatus.Duplicate => ColorResourceHelper.GetThemeColor(
                "Warning", "WarningDark", Color.FromArgb("#FF9500"), Color.FromArgb("#FF9F0A")),
            ImportPreviewRowStatus.Invalid => ColorResourceHelper.GetThemeColor(
                "Ausgabe", "AusgabeDark", Color.FromArgb("#FF3B30"), Color.FromArgb("#FF453A")),
            ImportPreviewRowStatus.Uncategorized => ColorResourceHelper.GetThemeColor(
                "ImportUncategorized", "ImportUncategorizedDark", Color.FromArgb("#FFCC00"), Color.FromArgb("#FFD60A")),
            ImportPreviewRowStatus.SaveError => ColorResourceHelper.GetThemeColor(
                "Ausgabe", "AusgabeDark", Color.FromArgb("#FF3B30"), Color.FromArgb("#FF453A")),
            _ => ColorResourceHelper.GetThemeColor(
                "Gray600", "Gray600", Color.FromArgb("#8E8E93"), Color.FromArgb("#8E8E93")),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
