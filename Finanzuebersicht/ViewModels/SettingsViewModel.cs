using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly ThemeService _themeService;
    private readonly ILogger<SettingsViewModel>? _logger;
    private readonly ILocalizationService _loc;
    private readonly IDialogService _dialogService;
    private readonly IBackupService? _backupService;
    private readonly Finanzuebersicht.Core.Services.IClock _clock;


    [ObservableProperty]
    private int selectedThemeIndex;

    [ObservableProperty]
    private int selectedLanguageIndex;

    [ObservableProperty]
    private string dataPath = string.Empty;

    [ObservableProperty]
    private string lastBackupInfo = string.Empty;

    public string AppVersion { get; }
    public string BuildInfo { get; }

    public List<LibraryInfo> Libraries { get; } = new();

    public SettingsViewModel(
        SettingsService settings,
        ThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialogService,
        IBackupService? backupService = null,
        ILogger<SettingsViewModel>? logger = null,
        Finanzuebersicht.Core.Services.IClock? clock = null)
    {
        _settings = settings;
        _themeService = themeService;
        _logger = logger;
        _loc = localizationService;
        _dialogService = dialogService;
        _backupService = backupService;
        _clock = clock ?? Finanzuebersicht.Core.Services.SystemClock.Instance;

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

        // Sprache laden
        var lang = _loc.CurrentLanguageCode;
        SelectedLanguageIndex = lang switch
        {
            "de" => 1,
            "en" => 2,
            _ => 0
        };

        // Datenpfad laden
        DataPath = _settings.Get("DataPath", "");
        if (string.IsNullOrWhiteSpace(DataPath))
        {
            DataPath = GetDefaultDataDir();
        }

        // Letztes Backup anzeigen
        UpdateLastBackupInfo();

        // Libraries dynamisch aus Referenzen befüllen
        PopulateLibraries();
    }

    private void PopulateLibraries()
    {
        try
        {
            var entry = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var refs = entry.GetReferencedAssemblies();

            foreach (var ra in refs.OrderBy(r => r.Name))
            {
                if (ra.Name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                    ra.Name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                    ra.Name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                    ra.Name.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Libraries.Add(new LibraryInfo(ra.Name, $"Version {ra.Version}"));
            }

            // Ergänzend: lade bekannte Assemblies, falls vorhanden
            var known = new[] { "CommunityToolkit.Mvvm", "CommunityToolkit.Maui", "Nerdbank.GitVersioning" };
            foreach (var name in known)
            {
                try
                {
                    var asm = Assembly.Load(new AssemblyName(name));
                    var n = asm.GetName();
                    if (!Libraries.Any(l => l.Name == n.Name))
                        Libraries.Add(new LibraryInfo(n.Name, $"Version {n.Version}"));
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Konnte bekannte Assembly '{Name}' nicht laden", name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Fehler beim Ermitteln der verwendeten Bibliotheken");
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
        _themeService.Apply(themeKey);
    }

    partial void OnSelectedLanguageIndexChanged(int value)
    {
        var code = value switch
        {
            1 => "de",
            2 => "en",
            _ => string.Empty  // Systemsprache
        };
        _loc.SetLanguage(string.IsNullOrEmpty(code) ? null : code);
    }

    [RelayCommand]
    private void SetLanguage(string indexStr)
    {
        if (int.TryParse(indexStr, out var idx))
            SelectedLanguageIndex = idx;
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
                    await _dialogService.ShowAlertAsync(
                        _loc.GetString(ResourceKeys.Stn_UngueltigerOrdner),
                        _loc.GetString(ResourceKeys.Stn_UngueltigerOrdnerDesc),
                        _loc.GetString(ResourceKeys.Btn_OK));
                    return;
                }

                _settings.Set("DataPath", newPath);
                DataPath = newPath;

                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Stn_SpeicherortGeaendert),
                    _loc.GetString(ResourceKeys.Stn_SpeicherortGeaendertDesc, newPath),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Err_OrdnerNichtWaehlbar, ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task ResetDataPath()
    {
        _settings.Set("DataPath", "");
        DataPath = GetDefaultDataDir();

        await _dialogService.ShowAlertAsync(
            _loc.GetString(ResourceKeys.Stn_SpeicherortZurueckgesetzt),
            _loc.GetString(ResourceKeys.Stn_SpeicherortZurueckgesetztDesc),
            _loc.GetString(ResourceKeys.Btn_OK));
    }

    private static string GetDefaultDataDir() => AppPaths.GetDefaultDataDir();

    private void UpdateLastBackupInfo()
    {
        var lastBackupStr = _settings.Get("LastBackupTime", "");
        if (string.IsNullOrEmpty(lastBackupStr))
        {
            LastBackupInfo = _loc.GetString(ResourceKeys.Stn_NoBackupYet);
        }
        else if (DateTime.TryParse(lastBackupStr, out var lastBackup))
        {
            var diff = _clock.UtcNow - lastBackup;
            if (diff.TotalSeconds < 60)
            {
                LastBackupInfo = _loc.GetString(ResourceKeys.Stn_LastBackupSeconds);
            }
            else if (diff.TotalMinutes < 60)
            {
                LastBackupInfo = string.Format(_loc.GetString(ResourceKeys.Stn_LastBackupMinutes), (int)diff.TotalMinutes);
            }
            else
            {
                LastBackupInfo = diff.TotalHours < 24
                ? string.Format(_loc.GetString(ResourceKeys.Stn_LastBackupHours), (int)diff.TotalHours)
                : string.Format(_loc.GetString(ResourceKeys.Stn_LastBackupDays), (int)diff.TotalDays);
            }
        }
        else
        {
            LastBackupInfo = _loc.GetString(ResourceKeys.Stn_NoBackupYet);
        }
    }

    [RelayCommand]
    private async Task CreateBackup()
    {
        if (_backupService == null)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Msg_BackupServiceNotAvailable),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            var backupPath = _settings.Get("BackupPath", "");
            if (string.IsNullOrEmpty(backupPath))
            {
                var dataPath = _settings.Get("DataPath", "");
                if (string.IsNullOrEmpty(dataPath))
                {
                    dataPath = AppPaths.GetDefaultDataDir();
                }

                backupPath = Path.Combine(dataPath, "backups");
            }

            var metadata = await _backupService.CreateBackupAsync(backupPath);

            UpdateLastBackupInfo();

            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_BackupSuccessTitle),
                string.Format(_loc.GetString(ResourceKeys.Msg_BackupCreatedBody), metadata.EntityCounts["categories"], metadata.EntityCounts["transactions"], metadata.EntityCounts["recurring"]),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_BackupFailedTitle),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task BrowseBackups()
    {
        if (_backupService == null)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Msg_BackupServiceNotAvailable),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            var backupPath = _settings.Get("BackupPath", "");
            if (string.IsNullOrEmpty(backupPath))
            {
                var dataPath = _settings.Get("DataPath", "");
                if (string.IsNullOrEmpty(dataPath))
                {
                    dataPath = AppPaths.GetDefaultDataDir();
                }

                backupPath = Path.Combine(dataPath, "backups");
            }

            var backups = (await _backupService.ListBackupsAsync(backupPath)).ToList();
            if (!backups.Any())
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_NoBackupsTitle),
                    _loc.GetString(ResourceKeys.Msg_NoBackupsDesc),
                    _loc.GetString(ResourceKeys.Btn_OK));
                return;
            }

            // Hier könnten wir zu einer separaten Backup-Verwaltungs-Page navigieren
            var backupList = string.Join("\n", backups.Take(5).Select(b => $"{b.CreatedAt:g} - {Path.GetFileNameWithoutExtension(b.FileName)}"));
            if (backups.Count > 5)
            {
                backupList += $"\n... und {backups.Count - 5} weitere";
            }

            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_AvailableBackupsTitle),
                backupList,
                _loc.GetString(ResourceKeys.Btn_OK));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        if (_backupService == null)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Msg_BackupServiceNotAvailable),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            var backupPath = _settings.Get("BackupPath", "");
            if (string.IsNullOrEmpty(backupPath))
            {
                var dataPath = _settings.Get("DataPath", "");
                if (string.IsNullOrEmpty(dataPath))
                {
                    dataPath = AppPaths.GetDefaultDataDir();
                }

                backupPath = Path.Combine(dataPath, "backups");
            }

            var backups = (await _backupService.ListBackupsAsync(backupPath)).ToList();
            if (!backups.Any())
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_NoBackupsTitle),
                    _loc.GetString(ResourceKeys.Msg_NoBackupsDesc),
                    _loc.GetString(ResourceKeys.Btn_OK));
                return;
            }

            // Hier könnten wir zu einer separaten Restore-Dialog-Page navigieren
            // Für nun: Informationen anzeigen
            var newestBackup = backups.First();
            var confirmed = await _dialogService.ShowConfirmationAsync(
                _loc.GetString(ResourceKeys.Msg_RestoreConfirmTitle),
                string.Format(_loc.GetString(ResourceKeys.Msg_RestoreConfirmBody), newestBackup.CreatedAt.ToString("g"), newestBackup.EntityCounts["transactions"]),
                _loc.GetString(ResourceKeys.Btn_Ja),
                _loc.GetString(ResourceKeys.Btn_Abbrechen));

            if (confirmed)
            {
                var result = await _backupService.RestoreBackupAsync(backupPath, newestBackup.Id);
                if (result.Success)
                {
                    await _dialogService.ShowAlertAsync(
                        _loc.GetString(ResourceKeys.Msg_RestoreSuccessTitle),
                        _loc.GetString(ResourceKeys.Msg_RestoreSuccessDesc),
                        _loc.GetString(ResourceKeys.Btn_OK));
                }
                else
                {
                    await _dialogService.ShowAlertAsync(
                        _loc.GetString(ResourceKeys.Msg_RestoreFailedTitle),
                        string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), result.ErrorMessage),
                        _loc.GetString(ResourceKeys.Btn_OK));
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    [RelayCommand]
    private async Task ExportAsCSV()
    {
        if (_backupService == null)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                _loc.GetString(ResourceKeys.Msg_BackupServiceNotAvailable),
                _loc.GetString(ResourceKeys.Btn_OK));
            return;
        }

        try
        {
            var csvStream = await _backupService.ExportAsCSVAsync();
            var csvData = new byte[csvStream.Length];
            csvStream.Seek(0, System.IO.SeekOrigin.Begin);
            csvStream.Read(csvData, 0, csvData.Length);

            var fileName = $"Finanzuebersicht_{_clock.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            File.WriteAllBytes(filePath, csvData);

            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_CSVExportedTitle),
                string.Format(_loc.GetString(ResourceKeys.Msg_CSVExportedBody), filePath),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_CSVExportFailedTitle),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }
}

public record LibraryInfo(string Name, string Description);
