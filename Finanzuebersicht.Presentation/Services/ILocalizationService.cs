using System.Globalization;

namespace Finanzuebersicht.Services;

public interface ILocalizationService
{
    /// <summary>Initialisiert ResourceManager und setzt gespeicherte oder Gerätesprache.</summary>
    void Initialize();

    /// <summary>Wechselt die Sprache live. null = Systemsprache verwenden.</summary>
    void SetLanguage(string? cultureCode);

    /// <summary>Aktueller Sprachcode (z.B. "de", "en") oder leer für Systemsprache.</summary>
    string CurrentLanguageCode { get; }

    /// <summary>Gibt den lokalisierten String zurück.</summary>
    string GetString(string key);

    /// <summary>Gibt den lokalisierten Format-String zurück, befüllt mit args.</summary>
    string GetString(string key, params object[] args);
}
