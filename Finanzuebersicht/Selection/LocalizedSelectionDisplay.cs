using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Selection;

public static class LocalizedSelectionDisplay
{
    public static string GetDisplayText(object? item, string displayMemberPath)
    {
        if (item is KategorieFilterItem { LocalizationResourceKey: { } key })
            return LocalizationResourceManager.Current[key];

        return SelectionDisplayHelper.GetDisplayText(item, displayMemberPath);
    }
}
