using Microsoft.Maui.Storage;

namespace Finanzuebersicht.Services;

/// <summary>
/// MAUI implementation: wraps <see cref="FilePicker"/> so ViewModels stay platform-agnostic.
/// </summary>
public class MauiFilePicker : IFilePicker
{
    public async Task<PickFileResult?> PickAsync()
    {
        var result = await FilePicker.Default.PickAsync();
        if (result is null) return null;
        return new PickFileResult(result.FileName, () => result.OpenReadAsync());
    }
}
