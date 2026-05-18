namespace Finanzuebersicht.Services;

public class MauiFileSaver : IFileSaver
{
    public async Task<FileSaverResult> SaveAsync(string fileName, Stream data, CancellationToken cancellationToken)
    {
        var result = await CommunityToolkit.Maui.Storage.FileSaver.Default.SaveAsync(fileName, data, cancellationToken);
        return new FileSaverResult(result.IsSuccessful, result.FilePath, result.Exception);
    }
}
