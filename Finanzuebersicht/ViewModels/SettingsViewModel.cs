using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;

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

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;

        // Version aus Assembly-Metadaten lesen (von Nerdbank.GitVersioning gesetzt)
        var asm = Assembly.GetExecutingAssembly();
        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unbekannt";
        var version = asm.GetName().Version;

        AppVersion = version is not null ? $"{version.Major}.{version.Minor}.{version.Build}" : infoVersion;
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
        {
            DataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Finanzuebersicht");
        }
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
        ApplyTheme(themeKey);
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
                _settings.Set("DataPath", newPath);
                DataPath = newPath;

                await Shell.Current.DisplayAlert(
                    "Speicherort geändert",
                    $"Daten werden ab dem nächsten Neustart unter\n{newPath}\ngespeichert.\n\nBitte starte die App neu.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler", $"Ordner konnte nicht gewählt werden: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ResetDataPath()
    {
        _settings.Set("DataPath", "");
        DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Finanzuebersicht");

        await Shell.Current.DisplayAlert(
            "Speicherort zurückgesetzt",
            "Daten werden ab dem nächsten Neustart im Standard-Verzeichnis gespeichert.",
            "OK");
    }

    public static void ApplyTheme(string themeKey)
    {
        if (Application.Current is null) return;

        Application.Current.UserAppTheme = themeKey switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

#if MACCATALYST || IOS
        // Sync UIKit interface style so native controls (nav bar, etc.) follow the theme
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var style = themeKey switch
            {
                "Light" => UIKit.UIUserInterfaceStyle.Light,
                "Dark" => UIKit.UIUserInterfaceStyle.Dark,
                _ => UIKit.UIUserInterfaceStyle.Unspecified
            };

            foreach (var scene in UIKit.UIApplication.SharedApplication.ConnectedScenes)
            {
                if (scene is UIKit.UIWindowScene windowScene)
                {
                    foreach (var window in windowScene.Windows)
                    {
                        window.OverrideUserInterfaceStyle = style;
                    }
                }
            }
        });
#endif
    }
}

public record LibraryInfo(string Name, string Description);
