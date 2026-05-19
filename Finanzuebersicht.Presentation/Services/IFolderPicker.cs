namespace Finanzuebersicht.Presentation.Services;

public interface IFolderPicker
{
    Task<string?> PickAsync();
}
