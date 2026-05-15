using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.TestHelpers;

/// <summary>
/// Creates an isolated SettingsService backed by a temp directory for the duration of a test.
/// Cleans up the directory on dispose.
/// </summary>
public sealed class SettingsScope : IDisposable
{
    private readonly string _directory;

    public SettingsScope(string testClassName, params (string Key, string Value)[] values)
    {
        _directory = Path.Combine(
            AppContext.BaseDirectory,
            "test-artifacts",
            testClassName,
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_directory);
        Settings = new SettingsService(Path.Combine(_directory, "settings.json"));

        foreach (var (key, value) in values)
            Settings.Set(key, value);
    }

    public SettingsService Settings { get; }

    public void Dispose()
    {
        try { Directory.Delete(_directory, true); } catch { }
    }
}
