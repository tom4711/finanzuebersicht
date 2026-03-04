namespace Finanzuebersicht.Services;

/// <summary>
/// Gemeinsamer Speicherort für den Standard-Datenpfad der App.
/// </summary>
public static class AppPaths
{
    public static string GetDefaultDataDir()
    {
        return GetDefaultDataDir(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst());
    }

    internal static string GetDefaultDataDir(string userProfilePath, string localApplicationDataPath, bool isMacLike)
    {
        // On macOS, .NET maps LocalApplicationData to ~/.local/share (Linux convention).
        // The correct macOS path is ~/Library/Application Support.
        if (isMacLike)
        {
            return Path.Combine(
                userProfilePath,
                "Library", "Application Support", "Finanzuebersicht");
        }

        return Path.Combine(localApplicationDataPath, "Finanzuebersicht");
    }
}
