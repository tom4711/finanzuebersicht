namespace Finanzuebersicht.Services;

/// <summary>
/// Verwaltet App-Einstellungen als Key-Value-Paare (JSON-Datei).
/// </summary>
public class SettingsService
{
    private static readonly string SettingsFile = Path.Combine(
        GetDefaultDataDir(), "settings.json");

    private static string GetDefaultDataDir()
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

    private Dictionary<string, string> _settings = [];
    private readonly object _lock = new();

    public SettingsService()
    {
        Load();
    }

    public string Get(string key, string defaultValue = "")
    {
        lock (_lock)
        {
            return _settings.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }

    public void Set(string key, string value)
    {
        lock (_lock)
        {
            _settings[key] = value;
            Save();
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                _settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }
        }
        catch
        {
            _settings = [];
        }
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsFile)!;
            Directory.CreateDirectory(dir);
            var json = System.Text.Json.JsonSerializer.Serialize(_settings,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch
        {
            // Fehler beim Speichern ignorieren
        }
    }
}
