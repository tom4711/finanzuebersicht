using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class BackupListViewModel : ObservableObject, IAutoLoadViewModel
{
    private readonly IBackupService _backupService;
    private readonly ISettingsService _settings;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly ILogger<BackupListViewModel>? _logger;

    public System.Windows.Input.ICommand AutoLoadCommand => LoadBackupsCommand;

    [ObservableProperty]
    private ObservableCollection<BackupMetadata> backups = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEmpty;

    public BackupListViewModel(
        IBackupService backupService,
        ISettingsService settings,
        IDialogService dialogService,
        ILocalizationService localizationService,
        INavigationService navigationService,
        ILogger<BackupListViewModel>? logger = null)
    {
        _backupService = backupService;
        _settings = settings;
        _dialogService = dialogService;
        _loc = localizationService;
        _navigationService = navigationService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadBackups()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var backupPath = GetBackupPath();
            var list = (await _backupService.ListBackupsAsync(backupPath)).ToList();
            Backups = new ObservableCollection<BackupMetadata>(list);
            IsEmpty = Backups.Count == 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "BackupListViewModel: {Context}", nameof(LoadBackups));
            Backups = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackup(BackupMetadata backup)
    {
        var dateDisplay = backup.CreatedAt.ToLocalTime().ToString("g");
        var confirmed = await _dialogService.ShowConfirmationAsync(
            _loc.GetString(ResourceKeys.Msg_BackupRestoreConfirmTitle),
            string.Format(_loc.GetString(ResourceKeys.Msg_BackupRestoreConfirmBody), dateDisplay),
            _loc.GetString(ResourceKeys.Btn_Ja),
            _loc.GetString(ResourceKeys.Btn_Abbrechen));

        if (!confirmed) return;

        try
        {
            var backupPath = GetBackupPath();
            var result = await _backupService.RestoreBackupAsync(backupPath, backup.Id);
            if (result.Success)
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_RestoreSuccessTitle),
                    _loc.GetString(ResourceKeys.Msg_RestoreSuccessDesc),
                    _loc.GetString(ResourceKeys.Btn_OK));
                await _navigationService.GoBackAsync();
            }
            else if (result.DataMayBeInconsistent)
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_RestoreInconsistentTitle),
                    _loc.GetString(ResourceKeys.Msg_RestoreInconsistentDesc),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
            else
            {
                await _dialogService.ShowAlertAsync(
                    _loc.GetString(ResourceKeys.Msg_RestoreFailedTitle),
                    string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), result.ErrorMessage ?? string.Empty),
                    _loc.GetString(ResourceKeys.Btn_OK));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "BackupListViewModel: RestoreBackup failed");
            await _dialogService.ShowAlertAsync(
                _loc.GetString(ResourceKeys.Err_Titel),
                string.Format(_loc.GetString(ResourceKeys.Err_SpeichernFehlgeschlagen), ex.Message),
                _loc.GetString(ResourceKeys.Btn_OK));
        }
    }

    private string GetBackupPath()
    {
        var backupPath = _settings.Get("BackupPath", "");
        if (!string.IsNullOrEmpty(backupPath)) return backupPath;

        var dataPath = _settings.Get("DataPath", "");
        if (string.IsNullOrEmpty(dataPath))
            dataPath = AppPaths.GetDefaultDataDir();

        return Path.Combine(dataPath, "backups");
    }
}
