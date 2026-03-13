namespace Finanzuebersicht.Services;

public class ShellNavigationService : INavigationService
{
    public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (Shell.Current is null) return;

        if (parameters is null)
        {
            await Shell.Current.GoToAsync(route);
        }
        else
        {
            await Shell.Current.GoToAsync(route, parameters);
        }
    }

    public async Task GoBackAsync()
    {
        if (Shell.Current is null) return;
        await Shell.Current.GoToAsync("..");
    }
}
