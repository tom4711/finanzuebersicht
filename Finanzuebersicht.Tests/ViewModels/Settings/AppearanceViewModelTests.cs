using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.ViewModels.Settings;

public class AppearanceViewModelTests
{
    [Fact]
    public void Constructor_LoadsThemeFromSettings()
    {
        using var settingsScope = new SettingsScope(("Theme", "Dark"));
        var themeService = Substitute.For<IThemeService>();
        var localizationService = CreateLocalizationService();

        var sut = new AppearanceViewModel(settingsScope.Settings, themeService, localizationService);

        Assert.Equal(2, sut.SelectedThemeIndex);
    }

    [Fact]
    public void Constructor_LoadsLanguageFromLocalizationService()
    {
        using var settingsScope = new SettingsScope();
        var themeService = Substitute.For<IThemeService>();
        var localizationService = CreateLocalizationService("en");

        var sut = new AppearanceViewModel(settingsScope.Settings, themeService, localizationService);

        Assert.Equal(2, sut.SelectedLanguageIndex);
    }

    [Fact]
    public void SetTheme_UpdatesIndexAndPersists()
    {
        using var settingsScope = new SettingsScope();
        var themeService = Substitute.For<IThemeService>();
        var localizationService = CreateLocalizationService();
        var sut = new AppearanceViewModel(settingsScope.Settings, themeService, localizationService);

        sut.SetThemeCommand.Execute("1");

        Assert.Equal(1, sut.SelectedThemeIndex);
        Assert.Equal("Light", settingsScope.Settings.Get("Theme"));
        themeService.Received(1).Apply("Light");
    }

    [Fact]
    public void OnSelectedThemeIndexChanged_AppliesTheme()
    {
        using var settingsScope = new SettingsScope();
        var themeService = Substitute.For<IThemeService>();
        var localizationService = CreateLocalizationService();
        var sut = new AppearanceViewModel(settingsScope.Settings, themeService, localizationService);

        sut.SelectedThemeIndex = 2;

        Assert.Equal("Dark", settingsScope.Settings.Get("Theme"));
        themeService.Received(1).Apply("Dark");
    }

    [Fact]
    public void SetLanguage_UpdatesIndex()
    {
        using var settingsScope = new SettingsScope();
        var themeService = Substitute.For<IThemeService>();
        var localizationService = CreateLocalizationService();
        var sut = new AppearanceViewModel(settingsScope.Settings, themeService, localizationService);

        sut.SetLanguageCommand.Execute("2");

        Assert.Equal(2, sut.SelectedLanguageIndex);
        localizationService.Received(1).SetLanguage("en");
    }

    private static ILocalizationService CreateLocalizationService(string currentLanguageCode = "")
    {
        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.CurrentLanguageCode.Returns(currentLanguageCode);
        return localizationService;
    }

    private sealed class SettingsScope : IDisposable
    {
        private readonly string directory;

        public SettingsScope(params (string Key, string Value)[] values)
        {
            directory = Path.Combine(
                AppContext.BaseDirectory,
                "test-artifacts",
                nameof(AppearanceViewModelTests),
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            Settings = new SettingsService(Path.Combine(directory, "settings.json"));

            foreach (var (key, value) in values)
            {
                Settings.Set(key, value);
            }
        }

        public SettingsService Settings { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
            }
        }
    }
}
