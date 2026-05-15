using CommunityToolkit.Mvvm.ComponentModel;

namespace Finanzuebersicht.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public AppearanceViewModel Appearance { get; }
    public StorageViewModel Storage { get; }
    public BackupViewModel Backup { get; }
    public AboutViewModel About { get; }

    public SettingsViewModel(
        AppearanceViewModel appearance,
        StorageViewModel storage,
        BackupViewModel backup,
        AboutViewModel about)
    {
        Appearance = appearance;
        Storage = storage;
        Backup = backup;
        About = about;
    }
}
