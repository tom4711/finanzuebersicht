namespace Finanzuebersicht.Services;

/// <summary>
/// Verwaltet App-Einstellungen als Key-Value-Paare (JSON-Datei).
/// </summary>
public class SettingsService
{
    private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Finanzuebersicht", "settings.json");

    private Dictionary<string, string> _settings = [];

    public SettingsService()
    {
        Load();
    }

    public string Get(string key, string defaultValue = "")
    {
        return _settings.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public void Set(string key, string value)
    {
        _settings[key] = value;
        Save();
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
