using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

/// <summary>
/// Base page that automatically executes <see cref="IAutoLoadViewModel.AutoLoadCommand"/>
/// on appearing. Pages with complex OnAppearing logic (e.g. DashboardPage) should
/// override directly instead of using this base class.
/// </summary>
public abstract class BaseContentPage : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is IAutoLoadViewModel vm && vm.ShouldAutoLoad)
            vm.AutoLoadCommand.Execute(null);
    }
}
