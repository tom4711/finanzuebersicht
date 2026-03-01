using Finanzuebersicht.Services;

namespace Finanzuebersicht;

/// <summary>
/// Global service locator for converters that need access to services
/// </summary>
public static class ServiceRegistry
{
    private static IDataService? _dataService;

    public static void Initialize(IDataService dataService)
    {
        _dataService = dataService;
    }

    public static IDataService? DataService => _dataService;
}
