namespace Finanzuebersicht.Services;

/// <summary>
/// Verwaltet App-Einstellungen als Key-Value-Paare (JSON-Datei).
/// </summary>
public class SettingsService
{
    private readonly string _settingsFile;

    private Dictionary<string, string> _settings = [];
    private readonly object _lock = new();

    public SettingsService()
        : this(Path.Combine(AppPaths.GetDefaultDataDir(), "settings.json")) { }

    internal SettingsService(string settingsFilePath)
    {
        _settingsFile = settingsFilePath;
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
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                _settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Einstellungen: {ex.Message}");
            _settings = [];
        }
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_settingsFile)!;
            Directory.CreateDirectory(dir);
            var json = System.Text.Json.JsonSerializer.Serialize(_settings,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Speichern der Einstellungen: {ex.Message}");
        }
    }
}
