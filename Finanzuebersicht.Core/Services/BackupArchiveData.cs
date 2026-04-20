namespace Finanzuebersicht.Services;

/// <summary>
/// Rohdaten eines Backup-Archivs als JSON-Strings pro Datei.
/// Wird für die Migration zwischen Schema-Versionen verwendet.
/// </summary>
public class BackupArchiveData
{
    /// <summary>
    /// Dateiname → JSON-Inhalt (z.B. "categories.json" → "[...]")
    /// </summary>
    public Dictionary<string, string> Files { get; set; } = new();

    /// <summary>
    /// Die Metadaten des Backups.
    /// </summary>
    public BackupMetadata Metadata { get; set; } = new();
}
