namespace Finanzuebersicht.Services;

public class MauiFolderPicker : IFolderPicker
{
    public async Task<string?> PickAsync()
    {
        var result = await CommunityToolkit.Maui.Storage.FolderPicker.Default.PickAsync();
        return result.IsSuccessful ? result.Folder?.Path : null;
    }
}
