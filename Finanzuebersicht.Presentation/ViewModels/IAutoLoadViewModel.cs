namespace Finanzuebersicht.ViewModels;

/// <summary>
/// ViewModels implementing this interface expose a single load command
/// that BaseContentPage calls automatically on OnAppearing.
/// </summary>
public interface IAutoLoadViewModel
{
    System.Windows.Input.ICommand AutoLoadCommand { get; }
    bool ShouldAutoLoad => true;
}
