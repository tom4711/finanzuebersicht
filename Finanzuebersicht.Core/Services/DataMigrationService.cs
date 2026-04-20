namespace Finanzuebersicht.Services;

/// <summary>
/// Verkettet Migrations-Schritte um ein Backup beliebiger Version
/// auf die aktuelle Schema-Version zu heben.
/// </summary>
public class DataMigrationService
{
    private readonly IReadOnlyList<IDataMigrator> _migrators;

    public DataMigrationService(IEnumerable<IDataMigrator> migrators)
    {
        // Sortiert nach FromVersion damit die Kette immer in der richtigen Reihenfolge läuft
        _migrators = [.. migrators.OrderBy(m => m.FromVersion)];
    }

    /// <summary>
    /// Gibt true zurück wenn das Backup migriert werden muss.
    /// </summary>
    public bool NeedsMigration(BackupMetadata metadata, int currentSchemaVersion)
        => metadata.SchemaVersion < currentSchemaVersion;

    /// <summary>
    /// Führt alle nötigen Migrations-Schritte durch und gibt die aktualisierten Daten zurück.
    /// </summary>
    public async Task<BackupArchiveData> MigrateAsync(BackupArchiveData data, int targetVersion)
    {
        var current = data;
        var version = current.Metadata.SchemaVersion;

        while (version < targetVersion)
        {
            var migrator = _migrators.FirstOrDefault(m => m.FromVersion == version)
                ?? throw new InvalidOperationException(
                    $"Kein Migrator für Schema-Version {version} → {version + 1} gefunden. " +
                    $"Backup kann nicht wiederhergestellt werden.");

            current = await migrator.MigrateAsync(current);
            current.Metadata.SchemaVersion = migrator.ToVersion;
            version = migrator.ToVersion;
        }

        return current;
    }
}
