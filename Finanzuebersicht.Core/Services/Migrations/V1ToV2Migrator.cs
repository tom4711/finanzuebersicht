namespace Finanzuebersicht.Services.Migrations;

/// <summary>
/// Migriert v1-Backups auf v2: ergänzt fehlende budgets.json und sparziele.json
/// mit leeren Arrays, da diese Daten-Typen in Schema-Version 1 noch nicht existierten.
/// </summary>
public class V1ToV2Migrator : IDataMigrator
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public Task<BackupArchiveData> MigrateAsync(BackupArchiveData data)
    {
        if (!data.Files.ContainsKey("budgets.json"))
            data.Files["budgets.json"] = "[]";

        if (!data.Files.ContainsKey("sparziele.json"))
            data.Files["sparziele.json"] = "[]";

        return Task.FromResult(data);
    }
}
