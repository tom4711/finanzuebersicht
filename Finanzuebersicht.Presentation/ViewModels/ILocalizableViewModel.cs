namespace Finanzuebersicht.ViewModels;

/// <summary>
/// ViewModels with strings that do not auto-update via <c>loc:Translate</c> bindings.
/// </summary>
public interface ILocalizableViewModel
{
    void RefreshLocalizedStrings();
}
