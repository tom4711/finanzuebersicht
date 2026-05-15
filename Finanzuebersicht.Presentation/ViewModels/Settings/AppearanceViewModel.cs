using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class AppearanceViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _loc;

    [ObservableProperty]
    private int selectedThemeIndex;

    [ObservableProperty]
    private int selectedLanguageIndex;

    public AppearanceViewModel(
        SettingsService settings,
        IThemeService themeService,
        ILocalizationService localizationService)
    {
        _settings = settings;
        _themeService = themeService;
        _loc = localizationService;

        var theme = _settings.Get("Theme", "System");
        selectedThemeIndex = theme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };

        var lang = _loc.CurrentLanguageCode;
        selectedLanguageIndex = lang switch
        {
            "de" => 1,
            "en" => 2,
            _ => 0
        };
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var themeKey = value switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "System"
        };

        _settings.Set("Theme", themeKey);
        _themeService.Apply(themeKey);
    }

    partial void OnSelectedLanguageIndexChanged(int value)
    {
        var code = value switch
        {
            1 => "de",
            2 => "en",
            _ => string.Empty
        };

        _loc.SetLanguage(string.IsNullOrEmpty(code) ? null : code);
    }

    [RelayCommand]
    private void SetTheme(string indexStr)
    {
        if (int.TryParse(indexStr, out var idx))
        {
            SelectedThemeIndex = idx;
        }
    }

    [RelayCommand]
    private void SetLanguage(string indexStr)
    {
        if (int.TryParse(indexStr, out var idx))
        {
            SelectedLanguageIndex = idx;
        }
    }
}
