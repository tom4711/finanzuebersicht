using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.ViewModels.Settings;

public class StorageViewModelTests
{
    [Fact]
    public void Constructor_LoadsDataPathFromSettings()
    {
        using var settingsScope = new SettingsScope(("DataPath", "/Users/test/custom-data"));

        var sut = new StorageViewModel(
            settingsScope.Settings,
            CreateDialogService(),
            CreateLocalizationService());

        Assert.Equal("/Users/test/custom-data", sut.DataPath);
    }

    [Fact]
    public void Constructor_UsesDefaultPath_WhenSettingsEmpty()
    {
        using var settingsScope = new SettingsScope(("DataPath", string.Empty));

        var sut = new StorageViewModel(
            settingsScope.Settings,
            CreateDialogService(),
            CreateLocalizationService());

        Assert.Equal(AppPaths.GetDefaultDataDir(), sut.DataPath);
        Assert.False(string.IsNullOrWhiteSpace(sut.DataPath));
    }

    [Fact]
    public async Task ResetDataPath_ClearsSettingsAndRestoresDefault()
    {
        using var settingsScope = new SettingsScope(("DataPath", "/Users/test/custom-data"));
        var dialogService = CreateDialogService();
        var sut = new StorageViewModel(
            settingsScope.Settings,
            dialogService,
            CreateLocalizationService());

        await sut.ResetDataPathCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, settingsScope.Settings.Get("DataPath"));
        Assert.Equal(AppPaths.GetDefaultDataDir(), sut.DataPath);
        await dialogService.Received(1).ShowAlertAsync(
            ResourceKeys.Stn_SpeicherortZurueckgesetzt,
            ResourceKeys.Stn_SpeicherortZurueckgesetztDesc,
            ResourceKeys.Btn_OK);
    }

    [Fact]
    public async Task ChooseDataPath_WhenPickerReturnsNull_DoesNothing()
    {
        using var settingsScope = new SettingsScope();
        var dialogService = CreateDialogService();
        var folderPicker = Substitute.For<IFolderPicker>();
        folderPicker.PickAsync().Returns(Task.FromResult<string?>(null));
        var sut = new StorageViewModel(
            settingsScope.Settings,
            dialogService,
            CreateLocalizationService(),
            folderPicker);

        await sut.ChooseDataPathCommand.ExecuteAsync(null);

        Assert.Equal("__missing__", settingsScope.Settings.Get("DataPath", "__missing__"));
        await dialogService.DidNotReceive().ShowAlertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ChooseDataPath_WhenPickerReturnsTempPath_ShowsError()
    {
        using var settingsScope = new SettingsScope();
        var dialogService = CreateDialogService();
        var folderPicker = Substitute.For<IFolderPicker>();
        folderPicker.PickAsync().Returns(Task.FromResult<string?>(Path.Combine(Path.GetTempPath(), "something")));
        var sut = new StorageViewModel(
            settingsScope.Settings,
            dialogService,
            CreateLocalizationService(),
            folderPicker);

        await sut.ChooseDataPathCommand.ExecuteAsync(null);

        Assert.Equal("__missing__", settingsScope.Settings.Get("DataPath", "__missing__"));
        await dialogService.Received(1).ShowAlertAsync(
            ResourceKeys.Stn_UngueltigerOrdner,
            ResourceKeys.Stn_UngueltigerOrdnerDesc,
            ResourceKeys.Btn_OK);
    }

    private static IDialogService CreateDialogService()
    {
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowAlertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
        return dialogService;
    }

    private static ILocalizationService CreateLocalizationService()
    {
        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString(Arg.Any<string>()).Returns(call => call.Arg<string>());
        localizationService.GetString(Arg.Any<string>(), Arg.Any<object[]>()).Returns(call => call.ArgAt<string>(0));
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
                nameof(StorageViewModelTests),
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
