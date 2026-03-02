using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SettingsService _settings;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"finanz_settings_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _settings = new SettingsService(Path.Combine(_tempDir, "settings.json"));
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Get_ReturnsDefaultWhenKeyMissing()
    {
        var result = _settings.Get("NonExistentKey_" + Guid.NewGuid(), "defaultValue");
        Assert.Equal("defaultValue", result);
    }

    [Fact]
    public void SetAndGet_RoundTrip()
    {
        var key = "TestKey_" + Guid.NewGuid();
        _settings.Set(key, "TestValue");
        Assert.Equal("TestValue", _settings.Get(key));
    }

    [Fact]
    public void Set_OverwritesExistingValue()
    {
        var key = "OverwriteKey_" + Guid.NewGuid();
        _settings.Set(key, "Initial");
        _settings.Set(key, "Updated");
        Assert.Equal("Updated", _settings.Get(key));
    }
}
