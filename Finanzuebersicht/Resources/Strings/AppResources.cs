using System.Resources;

namespace Finanzuebersicht.Resources.Strings;

/// <summary>
/// Provides access to the app's ResourceManager for use with LocalizationResourceManager.
/// </summary>
internal static class AppResources
{
    private static readonly ResourceManager _resourceManager =
        new("Finanzuebersicht.Resources.Strings.AppResources", typeof(AppResources).Assembly);

    public static ResourceManager ResourceManager => _resourceManager;
}
