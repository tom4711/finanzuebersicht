using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly IBackupService? _backupService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly IFileSaver? _fileSaver;
    private readonly IClock _clock;
    private readonly ILogger<BackupViewModel>? _logger;

    [ObservableProperty]
    private string lastBackupInfo = string.Empty;

    public BackupViewModel(
        SettingsService settings,
        IBackupService? backupService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        INavigationService navigationService,
        IFileSaver? fileSaver = null,
        IClock? clock = null,
        ILogger<BackupViewModel>? logger = null)
    {
        _settings = settings;
        _backupService = backupService;
        _dialogService = dialogService;
        _loc = localizationService;
        _navigationService = navigationService;
        _fileSaver = fileSaver;
        _clock = clock ?? SystemClock.Instance;
        _logger = logger;

        UpdateLastBackupInfo();
    }

    [RelayCommand]
    private async Task CreateBackup()
    {
        if (!await EnsureBackupServiceAvailableAsync()) return;

        var backupService = _backupService!;

        try
        {
            var metadata = await backupService.CreateBackupAsync(GetBackupPath());

            UpdateLastBackupInfo();

            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_BackupSuccessTitle),
                string.Format(
                    _loc.GetString(ResourceKeys.Msg_BackupCreatedBody),
                    metadata.EntityCounts["categories"],
                    metadata.EntityCounts["transactions"],
                    metadata.EntityCounts["recurring"]),
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
        if (!await EnsureBackupServiceAvailableAsync()) return;

        var backupService = _backupService!;

        try
        {
            var backups = (await backupService.ListBackupsAsync(GetBackupPath())).ToList();
            if (!backups.Any())
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_NoBackupsTitle),
                    _loc.GetString(ResourceKeys.Msg_NoBackupsDesc),
                    _loc.GetString(ResourceKeys.Btn_OK));
                return;
            }

            var backupList = string.Join("\n", backups.Take(5).Select(b => $"{b.CreatedAt:g} - {Path.GetFileNameWithoutExtension(b.FileName)}"));
            if (backups.Count > 5)
            {
                backupList += "\n" + _loc.GetString(ResourceKeys.Msg_AndMoreBackups, backups.Count - 5);
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
        if (!await EnsureBackupServiceAvailableAsync()) return;

        await _navigationService.GoToAsync(Routes.BackupList);
    }

    [RelayCommand]
    private async Task ExportAsCSV()
    {
        if (!await EnsureBackupServiceAvailableAsync()) return;

        var backupService = _backupService!;

        try
        {
            if (_fileSaver == null)
            {
                return;
            }

            await using var csvStream = await backupService.ExportAsCSVAsync();
            csvStream.Seek(0, SeekOrigin.Begin);

            var fileName = $"Finanzuebersicht_Export_{_clock.Now:yyyy-MM-dd}.csv";
            var result = await _fileSaver.SaveAsync(fileName, csvStream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_CSVExportedTitle),
                    string.Format(_loc.GetString(ResourceKeys.Msg_CSVExportedBody), result.FilePath),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
            else if (result.Exception is not null and not OperationCanceledException)
            {
                _logger?.LogError(result.Exception, "ExportAsCSV SaveAsync failed");
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_CSVExportFailedTitle),
                    string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), result.Exception.Message),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ExportAsCSV failed");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Msg_CSVExportFailedTitle),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    private async Task<bool> EnsureBackupServiceAvailableAsync()
    {
        if (_backupService != null) return true;

        await _dialogService.ShowAlertAsync(
            _loc.GetString(ResourceKeys.Err_Titel),
            _loc.GetString(ResourceKeys.Msg_BackupServiceNotAvailable),
            _loc.GetString(ResourceKeys.Btn_OK));
        return false;
    }

    private void UpdateLastBackupInfo()
    {
        var lastBackupStr = _settings.Get("LastBackupTime", string.Empty);
        if (string.IsNullOrEmpty(lastBackupStr))
        {
            LastBackupInfo = _loc.GetString(ResourceKeys.Stn_NoBackupYet);
            return;
        }

        if (!DateTime.TryParse(lastBackupStr, out var lastBackup))
        {
            LastBackupInfo = _loc.GetString(ResourceKeys.Stn_NoBackupYet);
            return;
        }

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

    private string GetBackupPath()
    {
        var backupPath = _settings.Get("BackupPath", string.Empty);
        if (!string.IsNullOrEmpty(backupPath))
        {
            return backupPath;
        }

        var dataPath = _settings.Get("DataPath", string.Empty);
        if (string.IsNullOrEmpty(dataPath))
        {
            dataPath = AppPaths.GetDefaultDataDir();
        }

        return Path.Combine(dataPath, "backups");
    }
}
