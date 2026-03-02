using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly ThemeService _themeService;
    private readonly ILocalizationService _loc;

    [ObservableProperty]
    private int selectedThemeIndex;

    [ObservableProperty]
    private string dataPath = string.Empty;

    public string AppVersion { get; }
    public string BuildInfo { get; }

    public List<LibraryInfo> Libraries { get; } =
    [
        new("CommunityToolkit.Mvvm", "MVVM-Toolkit mit Source Generators"),
        new("CommunityToolkit.Maui", "UI-Erweiterungen für .NET MAUI"),
        new("Nerdbank.GitVersioning", "Automatische SemVer-Versionierung"),
        new("xUnit", "Unit-Test-Framework (nur Tests)"),
    ];

    public SettingsViewModel(SettingsService settings, ThemeService themeService, ILocalizationService localizationService)
    {
        _settings = settings;
        _themeService = themeService;
        _loc = localizationService;

        // Version aus Assembly-Metadaten lesen (von Nerdbank.GitVersioning gesetzt)
        var asm = Assembly.GetExecutingAssembly();
        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unbekannt";

        // InformationalVersion format: "0.1.1+64089cc7a3" — extract version before '+'
        AppVersion = infoVersion.Contains('+') ? infoVersion[..infoVersion.IndexOf('+')] : infoVersion;
        BuildInfo = infoVersion.Contains('+') ? infoVersion[(infoVersion.IndexOf('+') + 1)..] : "";

        // Theme laden
        var theme = _settings.Get("Theme", "System");
        SelectedThemeIndex = theme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };

        // Datenpfad laden
        DataPath = _settings.Get("DataPath", "");
        if (string.IsNullOrWhiteSpace(DataPath))
            DataPath = GetDefaultDataDir();
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

    [RelayCommand]
    private void SetTheme(string indexStr)
    {
        if (int.TryParse(indexStr, out var idx))
            SelectedThemeIndex = idx;
    }

    [RelayCommand]
    private async Task ChooseDataPath()
    {
        // Auf macOS/iOS: Ordnerauswahl via FolderPicker (CommunityToolkit.Maui)
        try
        {
            var result = await CommunityToolkit.Maui.Storage.FolderPicker.Default.PickAsync();
            if (result.IsSuccessful && result.Folder != null)
            {
                var newPath = result.Folder.Path;

                // Temp-Pfade ablehnen – FolderPicker gibt auf macOS manchmal einen
                // Sandbox-Temp-Pfad zurück (/var/folders/.../T/GUID) statt dem echten Pfad.
                var tempPath = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
                if (newPath.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase) ||
                    newPath.Contains(Path.Combine("var", "folders"), StringComparison.OrdinalIgnoreCase))
                {
                    await Shell.Current.DisplayAlert(
                        _loc.GetString(ResourceKeys.Stn_UngueltigerOrdner),
                        _loc.GetString(ResourceKeys.Stn_UngueltigerOrdnerDesc),
                        _loc.GetString(ResourceKeys.Btn_OK));
                    return;
                }

                _settings.Set("DataPath", newPath);
                DataPath = newPath;

                await Shell.Current.DisplayAlert(
                    _loc.GetString(ResourceKeys.Stn_SpeicherortGeaendert),
                    _loc.GetString(ResourceKeys.Stn_SpeicherortGeaendertDesc, newPath),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(_loc.GetString(ResourceKeys.Err_Titel), _loc.GetString(ResourceKeys.Err_OrdnerNichtWaehlbar, ex.Message), _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task ResetDataPath()
    {
        _settings.Set("DataPath", "");
        DataPath = GetDefaultDataDir();

        await Shell.Current.DisplayAlert(
            _loc.GetString(ResourceKeys.Stn_SpeicherortZurueckgesetzt),
            _loc.GetString(ResourceKeys.Stn_SpeicherortZurueckgesetztDesc),
            _loc.GetString(ResourceKeys.Btn_OK));
    }

    private static string GetDefaultDataDir() => AppPaths.GetDefaultDataDir();
}

public record LibraryInfo(string Name, string Description);
