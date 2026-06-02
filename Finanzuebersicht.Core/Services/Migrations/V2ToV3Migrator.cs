namespace Finanzuebersicht.Core.Services.Migrations;

/// <summary>
/// Migriert v2-Backups auf v3: ergänzt accounts.json mit leerem Array.
/// </summary>
public class V2ToV3Migrator : IDataMigrator
{
    public int FromVersion => 2;
    public int ToVersion => 3;

    public Task<BackupArchiveData> MigrateAsync(BackupArchiveData data)
    {
        if (!data.Files.ContainsKey("accounts.json"))
            data.Files["accounts.json"] = "[]";

        return Task.FromResult(data);
    }
}
