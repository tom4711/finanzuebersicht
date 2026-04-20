using Xunit;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Core.Services.Migrations;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using System.IO.Compression;
using System.Text.Json;

namespace Finanzuebersicht.Tests.Services
{
    /// <summary>
    /// Integration tests for complete backup/restore cycles.
    /// Tests end-to-end scenarios with multiple backup operations, restores, and data validation.
    /// </summary>
    public class BackupRestoreIntegrationTests : IDisposable
    {
        private readonly string _testDir;
        private readonly MockDataService _mockDataService;
        private readonly MockSettingsService _mockSettingsService;

        public BackupRestoreIntegrationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"integration_tests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDir);
            _mockDataService = new MockDataService();
            _mockSettingsService = new MockSettingsService(_testDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                    Directory.Delete(_testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public async Task FullBackupRestoreCycle_PreserveAllData()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");

            // Setup data
            var originalCategories = new[]
            {
                new Category { Id = "c1", Name = "Groceries", Icon = "🛒", Color = "#FF5733" },
                new Category { Id = "c2", Name = "Transport", Icon = "🚗", Color = "#33FF57" }
            };

            var originalTransactions = new[]
            {
                new Transaction { Id = "t1", Titel = "Supermarket", Betrag = 50.5m, Datum = new DateTime(2026, 1, 1), KategorieId = "c1" },
                new Transaction { Id = "t2", Titel = "Gas", Betrag = 60.0m, Datum = new DateTime(2026, 1, 5), KategorieId = "c2" }
            };

            var originalRecurring = new[]
            {
                new RecurringTransaction
                {
                    Id = "r1",
                    Titel = "Rent",
                    Betrag = 1000m,
                    Typ = TransactionType.Ausgabe,
                    KategorieId = "c1",
                    Startdatum = new DateTime(2026, 1, 1),
                    Enddatum = null,
                    Aktiv = true
                }
            };

            _mockDataService.SetCategories(originalCategories);
            _mockDataService.SetTransactions(originalTransactions);
            _mockDataService.SetRecurring(originalRecurring);
            _mockDataService.SetBudgets([new CategoryBudget { Id = "b1", KategorieId = "c1", Betrag = 300m }]);
            _mockDataService.SetSparZiele([new SparZiel { Id = "s1", Titel = "Urlaub", ZielBetrag = 2000m }]);

            // Act 1: Create backup
            var backup = await service.CreateBackupAsync(backupPath);

            // Assert 1: Backup created with correct data
            Assert.NotNull(backup);
            Assert.Equal(2, backup.EntityCounts["categories"]);
            Assert.Equal(2, backup.EntityCounts["transactions"]);
            Assert.Equal(1, backup.EntityCounts["recurring"]);
            Assert.Equal(1, backup.EntityCounts["budgets"]);
            Assert.Equal(1, backup.EntityCounts["sparziele"]);
            Assert.True(File.Exists(Path.Combine(backupPath, backup.FileName)));

            // Act 2: Restore backup — clear state first to verify data is re-saved
            _mockDataService.SetCategories([]);
            _mockDataService.SetTransactions([]);
            _mockDataService.SetRecurring([]);
            _mockDataService.SetBudgets([]);
            _mockDataService.SetSparZiele([]);

            var restoreResult = await service.RestoreBackupAsync(backupPath, backup.Id);

            // Assert 2: Restore successful and data actually written back
            Assert.True(restoreResult.Success, restoreResult.ErrorMessage);
            Assert.NotNull(restoreResult.RestoredMetadata);
            Assert.Equal(backup.Id, restoreResult.RestoredMetadata.Id);

            var restoredCategories = await _mockDataService.GetCategoriesAsync();
            var restoredTransactions = await _mockDataService.GetTransactionsAsync(DateTime.MinValue, DateTime.MaxValue);
            var restoredRecurring = await _mockDataService.GetRecurringTransactionsAsync();
            var restoredBudgets = await _mockDataService.GetBudgetsAsync();
            var restoredSparziele = await _mockDataService.GetSparZieleAsync();

            Assert.Equal(2, restoredCategories.Count);
            Assert.Equal(2, restoredTransactions.Count);
            Assert.Single(restoredRecurring);
            Assert.Single(restoredBudgets);
            Assert.Single(restoredSparziele);
            Assert.Contains(restoredCategories, c => c.Id == "c1" && c.Name == "Groceries");
            Assert.Contains(restoredTransactions, t => t.Id == "t1" && t.Betrag == 50.5m);
            Assert.Contains(restoredRecurring, r => r.Id == "r1" && r.Titel == "Rent");
            Assert.Contains(restoredBudgets, b => b.Id == "b1" && b.Betrag == 300m);
            Assert.Contains(restoredSparziele, s => s.Id == "s1" && s.Titel == "Urlaub");
        }

        [Fact]
        public async Task MultipleBackups_ListsInCorrectOrder()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");
            var categories = new[] { new Category { Id = "1", Name = "Test", Icon = "🏠", Color = "#000" } };
            _mockDataService.SetCategories(categories);

            // Act: Create multiple backups with delays
            var backup1 = await service.CreateBackupAsync(backupPath);
            await Task.Delay(50);
            var backup2 = await service.CreateBackupAsync(backupPath);
            await Task.Delay(50);
            var backup3 = await service.CreateBackupAsync(backupPath);

            var backups = (await service.ListBackupsAsync(backupPath)).ToList();

            // Assert: Newest first
            Assert.Equal(3, backups.Count);
            Assert.True(backups[0].CreatedAt >= backups[1].CreatedAt);
            Assert.True(backups[1].CreatedAt >= backups[2].CreatedAt);
        }

        [Fact]
        public async Task BackupWithEmptyDatabase_CreatesValidBackup()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");
            // No data set

            // Act
            var backup = await service.CreateBackupAsync(backupPath);

            // Assert
            Assert.NotNull(backup);
            Assert.Equal(0, backup.EntityCounts["categories"]);
            Assert.Equal(0, backup.EntityCounts["transactions"]);
            Assert.Equal(0, backup.EntityCounts["recurring"]);
        }

        [Fact]
        public async Task DeleteBackup_RemovesBackupFile()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");
            var category = new Category { Id = "1", Name = "Test", Icon = "🏠", Color = "#000" };
            _mockDataService.SetCategories(new[] { category });

            var backup = await service.CreateBackupAsync(backupPath);
            var backupFile = Path.Combine(backupPath, backup.FileName);
            Assert.True(File.Exists(backupFile));

            // Act
            await service.DeleteBackupAsync(backupPath, backup.Id);

            // Assert
            Assert.False(File.Exists(backupFile));
        }

        [Fact]
        public async Task ExportAsCSV_ContainsAllTransactionData()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var categories = new[]
            {
                new Category { Id = "c1", Name = "Food", Icon = "🍕", Color = "#FF0000" }
            };
            var transactions = new[]
            {
                new Transaction
                {
                    Id = "t1",
                    Titel = "Pizza",
                    Betrag = 25.50m,
                    Datum = new DateTime(2026, 3, 10),
                    KategorieId = "c1",
                    Typ = TransactionType.Ausgabe,
                    Verwendungszweck = "Dinner with \"quotes\""
                }
            };

            _mockDataService.SetCategories(categories);
            _mockDataService.SetTransactions(transactions);

            // Act
            var csvStream = await service.ExportAsCSVAsync();
            using var reader = new StreamReader(csvStream);
            var csv = await reader.ReadToEndAsync();

            // Assert: CSV contains header and transaction
            Assert.Contains("Datum,Titel,Betrag,Typ,Kategorie,Verwendungszweck", csv);
            Assert.Contains("2026-03-10", csv);
            Assert.Contains("Pizza", csv);
            Assert.Contains("Food", csv);
            // Betrag kann "25.5" oder "25,5" sein je nach Kultur
            Assert.True(csv.Contains("25.5") || csv.Contains("25,5"), "CSV should contain the amount in either US or DE format");
        }

        [Fact]
        public async Task BackupSettings_PersistsBackupMetadata()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");
            _mockDataService.SetCategories(new[] { new Category { Id = "1", Name = "Test", Icon = "🏠", Color = "#000" } });

            // Act
            var backup1 = await service.CreateBackupAsync(backupPath);
            var lastBackupTime1 = _mockSettingsService.Get("LastBackupTime");

            await Task.Delay(100);

            var backup2 = await service.CreateBackupAsync(backupPath);
            var lastBackupTime2 = _mockSettingsService.Get("LastBackupTime");

            // Assert
            Assert.NotNull(lastBackupTime1);
            Assert.NotNull(lastBackupTime2);
            Assert.NotEqual(lastBackupTime1, lastBackupTime2);
        }

        [Fact]
        public async Task RestoreNonexistentBackup_ReturnsFailed()
        {
            // Arrange
            var service = new BackupService(_mockDataService, _mockSettingsService, new DataMigrationService([new V1ToV2Migrator()]));
            var backupPath = Path.Combine(_testDir, "backups");

            // Act
            var result = await service.RestoreBackupAsync(backupPath, "nonexistent-backup-id");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("nicht gefunden", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        // Mock implementations
        private class MockDataService : IDataService
        {
            private List<Category> _categories = [];
            private List<Transaction> _transactions = [];
            private List<RecurringTransaction> _recurring = [];
            private List<CategoryBudget> _budgets = [];
            private List<SparZiel> _sparziele = [];

            public void SetCategories(IEnumerable<Category> categories) => _categories = categories.ToList();
            public void SetTransactions(IEnumerable<Transaction> transactions) => _transactions = transactions.ToList();
            public void SetRecurring(IEnumerable<RecurringTransaction> recurring) => _recurring = recurring.ToList();
            public void SetBudgets(IEnumerable<CategoryBudget> budgets) => _budgets = budgets.ToList();
            public void SetSparZiele(IEnumerable<SparZiel> sparziele) => _sparziele = sparziele.ToList();

            public Task<List<Category>> GetCategoriesAsync() => Task.FromResult(_categories.ToList());
            public Task SaveCategoryAsync(Category category)
            {
                var idx = _categories.FindIndex(c => c.Id == category.Id);
                if (idx >= 0) _categories[idx] = category; else _categories.Add(category);
                return Task.CompletedTask;
            }
            public Task DeleteCategoryAsync(string id) { _categories.RemoveAll(c => c.Id == id); return Task.CompletedTask; }
            public Task ReplaceAllCategoriesAsync(IEnumerable<Category> categories) { _categories = categories.ToList(); return Task.CompletedTask; }

            public Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
                => Task.FromResult(_transactions.Where(t => t.Datum >= vonDatum && t.Datum <= bisDatum).ToList());
            public Task SaveTransactionAsync(Transaction transaction)
            {
                var idx = _transactions.FindIndex(t => t.Id == transaction.Id);
                if (idx >= 0) _transactions[idx] = transaction; else _transactions.Add(transaction);
                return Task.CompletedTask;
            }
            public Task DeleteTransactionAsync(string id) { _transactions.RemoveAll(t => t.Id == id); return Task.CompletedTask; }
            public Task ReplaceAllTransactionsAsync(IEnumerable<Transaction> transactions) { _transactions = transactions.ToList(); return Task.CompletedTask; }
            public Task<Category?> GetMostCommonCategoryForPayeeAsync(
                string payee,
                double confidenceThreshold = 0.5,
                CancellationToken cancellationToken = default)
                => Task.FromResult<Category?>(null);

            public Task<List<RecurringTransaction>> GetRecurringTransactionsAsync() => Task.FromResult(_recurring.ToList());
            public Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
            {
                var idx = _recurring.FindIndex(r => r.Id == recurring.Id);
                if (idx >= 0) _recurring[idx] = recurring; else _recurring.Add(recurring);
                return Task.CompletedTask;
            }
            public Task DeleteRecurringTransactionAsync(string id) { _recurring.RemoveAll(r => r.Id == id); return Task.CompletedTask; }
            public Task ReplaceAllRecurringTransactionsAsync(IEnumerable<RecurringTransaction> recurring) { _recurring = recurring.ToList(); return Task.CompletedTask; }

            public Task GeneratePendingRecurringTransactionsAsync() => Task.CompletedTask;

            public Task<YearSummary> GetYearSummaryAsync(int year) => Task.FromResult(new YearSummary());
            public Task<MonthSummary> GetMonthSummaryAsync(int year, int month) => Task.FromResult(new MonthSummary());

            public Task<List<CategoryBudget>> GetBudgetsAsync() => Task.FromResult(_budgets.ToList());
            public Task SaveBudgetAsync(CategoryBudget budget)
            {
                var idx = _budgets.FindIndex(b => b.Id == budget.Id);
                if (idx >= 0) _budgets[idx] = budget; else _budgets.Add(budget);
                return Task.CompletedTask;
            }
            public Task DeleteBudgetAsync(string id) { _budgets.RemoveAll(b => b.Id == id); return Task.CompletedTask; }
            public Task<CategoryBudget?> GetBudgetForCategoryAsync(string kategorieId, int year, int month) => Task.FromResult<CategoryBudget?>(null);
            public Task ReplaceAllBudgetsAsync(IEnumerable<CategoryBudget> budgets) { _budgets = budgets.ToList(); return Task.CompletedTask; }

            public Task<List<SparZiel>> GetSparZieleAsync() => Task.FromResult(_sparziele.ToList());
            public Task SaveSparZielAsync(SparZiel sparZiel)
            {
                var idx = _sparziele.FindIndex(s => s.Id == sparZiel.Id);
                if (idx >= 0) _sparziele[idx] = sparZiel; else _sparziele.Add(sparZiel);
                return Task.CompletedTask;
            }
            public Task DeleteSparZielAsync(string id) { _sparziele.RemoveAll(s => s.Id == id); return Task.CompletedTask; }
            public Task ReplaceAllSparZieleAsync(IEnumerable<SparZiel> sparziele) { _sparziele = sparziele.ToList(); return Task.CompletedTask; }
        }

        private class MockSettingsService : SettingsService
        {
            public MockSettingsService(string testDataDir) : base(Path.Combine(testDataDir, "settings.json"))
            {
            }
        }
    }
}
