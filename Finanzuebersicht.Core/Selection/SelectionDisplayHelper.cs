using System.Reflection;

namespace Finanzuebersicht.Selection;

public static class SelectionDisplayHelper
{
    public static string GetDisplayText(object? item, string displayMemberPath)
    {
        if (item is null)
            return string.Empty;

        if (string.IsNullOrWhiteSpace(displayMemberPath))
            return item.ToString() ?? string.Empty;

        var property = item.GetType().GetProperty(displayMemberPath, BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
    }
}
