namespace Finanzuebersicht.Services;

/// <summary>
/// Gemeinsamer Speicherort für den Standard-Datenpfad der App.
/// </summary>
public static class AppPaths
{
    public static string GetDefaultDataDir()
    {
        // On macOS, .NET maps LocalApplicationData to ~/.local/share (Linux convention).
        // The correct macOS path is ~/Library/Application Support.
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "Finanzuebersicht");
        }
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Finanzuebersicht");
    }
}
