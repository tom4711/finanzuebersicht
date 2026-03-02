using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Finanzuebersicht.Services;

/// <summary>
/// Singleton-Manager für Lokalisierungsressourcen.
/// Implementiert INotifyPropertyChanged – Bindings in XAML aktualisieren sich
/// automatisch beim Sprachwechsel (null als PropertyName = alle Properties).
/// </summary>
public sealed class LocalizationResourceManager : INotifyPropertyChanged
{
    private ResourceManager? _resourceManager;
    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    public static LocalizationResourceManager Current { get; } = new();

    private LocalizationResourceManager() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gibt den lokalisierten String für den angegebenen Key zurück.</summary>
    public string this[string key]
        => _resourceManager?.GetString(key, _currentCulture) ?? key;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            _currentCulture = value;
            // null löst alle Bindings neu aus
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    /// <summary>Muss vor dem ersten UI-Aufbau aufgerufen werden.</summary>
    public void Init(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }
}
