namespace Finanzuebersicht.Services;

/// <summary>Result of a file pick operation, abstracting MAUI's FileResult.</summary>
public sealed class PickFileResult
{
    private readonly Func<Task<Stream>> _openStreamAsync;

    public PickFileResult(string fileName, Func<Task<Stream>> openStreamAsync)
    {
        FileName = fileName;
        _openStreamAsync = openStreamAsync;
    }

    public string FileName { get; }

    public Task<Stream> OpenReadAsync() => _openStreamAsync();
}

/// <summary>
/// Abstracts FilePicker.PickAsync so ViewModels remain testable without a MAUI runtime.
/// </summary>
public interface IFilePicker
{
    Task<PickFileResult?> PickAsync();
}
