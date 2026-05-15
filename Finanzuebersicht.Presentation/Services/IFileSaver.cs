namespace Finanzuebersicht.Services;

public record FileSaverResult(bool IsSuccessful, string? FilePath, Exception? Exception);

public interface IFileSaver
{
    Task<FileSaverResult> SaveAsync(string fileName, Stream data, CancellationToken cancellationToken);
}
