namespace Finanzuebersicht.Services;

public interface IFolderPicker
{
    Task<string?> PickAsync();
}
