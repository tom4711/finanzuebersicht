namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Migriert Backup-Daten von einer Schema-Version zur nächsten.
/// </summary>
public interface IDataMigrator
{
    /// <summary>Schema-Version die als Eingabe erwartet wird.</summary>
    int FromVersion { get; }

    /// <summary>Schema-Version die nach der Migration vorliegt.</summary>
    int ToVersion { get; }

    /// <summary>
    /// Wendet die Migration auf die Backup-Daten an.
    /// Gibt die migrierten Daten zurück (darf dieselbe Instanz sein).
    /// </summary>
    Task<BackupArchiveData> MigrateAsync(BackupArchiveData data);
}
