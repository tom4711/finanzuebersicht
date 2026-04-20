using Finanzuebersicht.Services;
using Finanzuebersicht.Services.Migrations;

namespace Finanzuebersicht.Tests.Services;

public class DataMigrationTests
{
    // --- V1ToV2Migrator ---

    [Fact]
    public async Task V1ToV2_AddsBudgets_WhenMissing()
    {
        var migrator = new V1ToV2Migrator();
        var data = new BackupArchiveData
        {
            Metadata = new BackupMetadata { SchemaVersion = 1 },
            Files = { ["categories.json"] = "[]", ["transactions.json"] = "[]" }
        };

        var result = await migrator.MigrateAsync(data);

        Assert.True(result.Files.ContainsKey("budgets.json"));
        Assert.Equal("[]", result.Files["budgets.json"]);
    }

    [Fact]
    public async Task V1ToV2_AddsSparZiele_WhenMissing()
    {
        var migrator = new V1ToV2Migrator();
        var data = new BackupArchiveData
        {
            Metadata = new BackupMetadata { SchemaVersion = 1 },
            Files = { ["categories.json"] = "[]" }
        };

        var result = await migrator.MigrateAsync(data);

        Assert.True(result.Files.ContainsKey("sparziele.json"));
        Assert.Equal("[]", result.Files["sparziele.json"]);
    }

    [Fact]
    public async Task V1ToV2_PreservesExistingBudgets_WhenPresent()
    {
        var migrator = new V1ToV2Migrator();
        const string existingBudgets = "[{\"id\":\"1\"}]";
        var data = new BackupArchiveData
        {
            Metadata = new BackupMetadata { SchemaVersion = 1 },
            Files = { ["budgets.json"] = existingBudgets, ["sparziele.json"] = "[]" }
        };

        var result = await migrator.MigrateAsync(data);

        Assert.Equal(existingBudgets, result.Files["budgets.json"]);
    }

    [Fact]
    public void V1ToV2_HasCorrectVersionNumbers()
    {
        var migrator = new V1ToV2Migrator();
        Assert.Equal(1, migrator.FromVersion);
        Assert.Equal(2, migrator.ToVersion);
    }

    // --- DataMigrationService ---

    [Fact]
    public void NeedsMigration_ReturnsTrue_WhenBackupVersionOlderThanCurrent()
    {
        var service = new DataMigrationService([new V1ToV2Migrator()]);
        var metadata = new BackupMetadata { SchemaVersion = 1 };
        Assert.True(service.NeedsMigration(metadata, currentSchemaVersion: 2));
    }

    [Fact]
    public void NeedsMigration_ReturnsFalse_WhenBackupVersionCurrent()
    {
        var service = new DataMigrationService([new V1ToV2Migrator()]);
        var metadata = new BackupMetadata { SchemaVersion = 2 };
        Assert.False(service.NeedsMigration(metadata, currentSchemaVersion: 2));
    }

    [Fact]
    public async Task MigrateAsync_UpdatesSchemaVersion_AfterMigration()
    {
        var service = new DataMigrationService([new V1ToV2Migrator()]);
        var data = new BackupArchiveData
        {
            Metadata = new BackupMetadata { SchemaVersion = 1 },
            Files = { ["categories.json"] = "[]" }
        };

        var result = await service.MigrateAsync(data, targetVersion: 2);

        Assert.Equal(2, result.Metadata.SchemaVersion);
    }

    [Fact]
    public async Task MigrateAsync_ThrowsInvalidOperation_WhenNoMigratorFound()
    {
        var service = new DataMigrationService([]); // keine Migratoren
        var data = new BackupArchiveData
        {
            Metadata = new BackupMetadata { SchemaVersion = 1 },
            Files = { }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.MigrateAsync(data, targetVersion: 2));
    }
}
