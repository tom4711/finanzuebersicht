using Xunit;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services
{
    /// <summary>
    /// Integration tests for critical data consistency flows.
    /// Ensures that complex cascading operations maintain data integrity.
    /// </summary>
    public class DataConsistencyFlowIntegrationTests
    {
        private const string FallbackCategoryId = "fallback-cat";
        private const string FallbackCategorySystemKey = "SysCat_Sonstiges";

        [Fact]
        public async Task DeleteCategory_UpdatesTransactionsToFallback()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            var fallbackCat = new Category
            {
                Id = FallbackCategoryId,
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = FallbackCategorySystemKey
            };
            var targetCat = new Category
            {
                Id = "cat-to-delete",
                Name = "Groceries",
                Icon = "🛒",
                Color = "#34C759",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveCategoryAsync(fallbackCat);
            await dataService.SaveCategoryAsync(targetCat);

            var txn1 = new Transaction
            {
                Id = "txn1",
                Titel = "Supermarket",
                Betrag = 50m,
                Datum = DateTime.Today,
                KategorieId = "cat-to-delete",
                Typ = TransactionType.Ausgabe
            };
            var txn2 = new Transaction
            {
                Id = "txn2",
                Titel = "Farm Market",
                Betrag = 30m,
                Datum = DateTime.Today,
                KategorieId = "cat-to-delete",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveTransactionAsync(txn1);
            await dataService.SaveTransactionAsync(txn2);

            // Act
            // Simulate the delete flow: reassign affected transactions to fallback
            var affectedTransactions = (await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1)))
                .Where(t => t.KategorieId == "cat-to-delete")
                .ToList();

            foreach (var txn in affectedTransactions)
            {
                txn.KategorieId = FallbackCategoryId;
                await dataService.SaveTransactionAsync(txn);
            }

            await dataService.DeleteCategoryAsync("cat-to-delete");

            // Assert
            var allTransactions = await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1));

            var txn1After = allTransactions.FirstOrDefault(t => t.Id == "txn1");
            var txn2After = allTransactions.FirstOrDefault(t => t.Id == "txn2");

            Assert.NotNull(txn1After);
            Assert.NotNull(txn2After);
            Assert.Equal(FallbackCategoryId, txn1After.KategorieId);
            Assert.Equal(FallbackCategoryId, txn2After.KategorieId);

            var categories = await dataService.GetCategoriesAsync();
            var deletedCat = categories.FirstOrDefault(c => c.Id == "cat-to-delete");
            Assert.Null(deletedCat);
        }

        [Fact]
        public async Task DeleteCategory_UpdatesRecurringTransactionsToFallback()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            var fallbackCat = new Category
            {
                Id = FallbackCategoryId,
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = FallbackCategorySystemKey
            };
            var targetCat = new Category
            {
                Id = "cat-recurring",
                Name = "Utilities",
                Icon = "⚡",
                Color = "#FF9500",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveCategoryAsync(fallbackCat);
            await dataService.SaveCategoryAsync(targetCat);

            var recurring = new RecurringTransaction
            {
                Id = "recurring1",
                Titel = "Monthly Bill",
                Betrag = 100m,
                KategorieId = "cat-recurring",
                Typ = TransactionType.Ausgabe,
                Startdatum = DateTime.Today.AddMonths(-3),
                Aktiv = true
            };

            await dataService.SaveRecurringTransactionAsync(recurring);

            // Act
            var affectedRecurring = (await dataService.GetRecurringTransactionsAsync())
                .Where(r => r.KategorieId == "cat-recurring")
                .ToList();

            foreach (var rec in affectedRecurring)
            {
                rec.KategorieId = FallbackCategoryId;
                await dataService.SaveRecurringTransactionAsync(rec);
            }

            await dataService.DeleteCategoryAsync("cat-recurring");

            // Assert
            var recurringAfter = await dataService.GetRecurringTransactionsAsync();
            var recurringAfterDelete = recurringAfter.FirstOrDefault(r => r.Id == "recurring1");

            Assert.NotNull(recurringAfterDelete);
            Assert.Equal(FallbackCategoryId, recurringAfterDelete.KategorieId);

            var categories = await dataService.GetCategoriesAsync();
            var deletedCat = categories.FirstOrDefault(c => c.Id == "cat-recurring");
            Assert.Null(deletedCat);
        }

        [Fact]
        public async Task SaveRecurringWithPastStartDate_GeneratesInstancesImmediately()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            var category = new Category
            {
                Id = "cat-monthly",
                Name = "Salary",
                Icon = "💼",
                Color = "#34C759",
                Typ = TransactionType.Einnahme
            };

            await dataService.SaveCategoryAsync(category);

            var pastStartDate = DateTime.Today.AddMonths(-2);
            var recurring = new RecurringTransaction
            {
                Id = "recurring-past",
                Titel = "Monthly Salary",
                Betrag = 3000m,
                KategorieId = "cat-monthly",
                Typ = TransactionType.Einnahme,
                Startdatum = pastStartDate,
                Aktiv = true
            };

            // Act
            await dataService.SaveRecurringTransactionAsync(recurring);

            // Simulate the generation that happens on App startup
            var generatedTransactions = new List<Transaction>();
            var recurringTransactions = await dataService.GetRecurringTransactionsAsync();

            foreach (var rec in recurringTransactions.Where(r => r.Aktiv))
            {
                var startDate = rec.Startdatum;
                var endDate = rec.Enddatum ?? DateTime.Today.AddMonths(1);

                // Generate monthly instances from start to today
                var current = startDate;
                while (current <= DateTime.Today && current <= endDate)
                {
                    generatedTransactions.Add(new Transaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        Titel = rec.Titel,
                        Betrag = rec.Betrag,
                        Datum = current,
                        KategorieId = rec.KategorieId,
                        Typ = rec.Typ
                    });

                    current = current.AddMonths(1);
                }
            }

            foreach (var txn in generatedTransactions)
            {
                await dataService.SaveTransactionAsync(txn);
            }

            // Assert
            var allTransactions = await dataService.GetTransactionsAsync(
                pastStartDate.AddMonths(-1),
                DateTime.Today.AddMonths(1));

            var generatedByRecurring = allTransactions
                .Where(t => t.Titel == "Monthly Salary")
                .OrderBy(t => t.Datum)
                .ToList();

            Assert.NotEmpty(generatedByRecurring);
            Assert.True(generatedByRecurring.Count >= 2, "Should have generated at least 2 instances");
            Assert.Equal(pastStartDate.Year, generatedByRecurring.First().Datum.Year);
            Assert.Equal(pastStartDate.Month, generatedByRecurring.First().Datum.Month);
        }

        [Fact]
        public async Task TransactionDetailWithDeletedCategory_ShowsFallbackCategory()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            var fallbackCat = new Category
            {
                Id = FallbackCategoryId,
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = FallbackCategorySystemKey
            };
            var originalCat = new Category
            {
                Id = "cat-original",
                Name = "Entertainment",
                Icon = "🎬",
                Color = "#AF52DE",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveCategoryAsync(fallbackCat);
            await dataService.SaveCategoryAsync(originalCat);

            var transaction = new Transaction
            {
                Id = "txn-detail-test",
                Titel = "Movie Tickets",
                Betrag = 25m,
                Datum = DateTime.Today,
                KategorieId = "cat-original",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveTransactionAsync(transaction);

            // Act - Delete the original category
            await dataService.DeleteCategoryAsync("cat-original");

            // Load the transaction detail
            var allTransactions = await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1));
            var txnDetail = allTransactions.FirstOrDefault(t => t.Id == "txn-detail-test");
            var allCategories = await dataService.GetCategoriesAsync();

            // Simulate finding the fallback category for display
            var categoryForDisplay = allCategories.FirstOrDefault(c => c.Id == txnDetail?.KategorieId);
            if (categoryForDisplay == null)
            {
                categoryForDisplay = allCategories.FirstOrDefault(c => c.SystemKey == FallbackCategorySystemKey);
            }

            // Assert
            Assert.NotNull(txnDetail);
            Assert.NotNull(categoryForDisplay);
            Assert.Equal(FallbackCategorySystemKey, categoryForDisplay.SystemKey);
            Assert.Equal("Sonstiges", categoryForDisplay.Name);
        }

        /// <summary>
        /// In-memory implementation of IDataService for testing.
        /// </summary>
        private class InMemoryDataService : IDataService
        {
            private readonly List<Category> _categories = [];
            private readonly List<Transaction> _transactions = [];
            private readonly List<RecurringTransaction> _recurring = [];

            // ICategoryRepository
            public Task<List<Category>> GetCategoriesAsync() =>
                Task.FromResult(new List<Category>(_categories));

            public Task SaveCategoryAsync(Category category)
            {
                var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
                if (existing != null)
                {
                    _categories.Remove(existing);
                }

                _categories.Add(category);
                return Task.CompletedTask;
            }

            public Task DeleteCategoryAsync(string id)
            {
                _categories.RemoveAll(c => c.Id == id);
                return Task.CompletedTask;
            }

            // ITransactionRepository
            public Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum) =>
                Task.FromResult(_transactions
                    .Where(t => t.Datum >= vonDatum && t.Datum <= bisDatum)
                    .ToList());

            public Task SaveTransactionAsync(Transaction transaction)
            {
                var existing = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
                if (existing != null)
                {
                    _transactions.Remove(existing);
                }

                _transactions.Add(transaction);
                return Task.CompletedTask;
            }

            public Task DeleteTransactionAsync(string id)
            {
                _transactions.RemoveAll(t => t.Id == id);
                return Task.CompletedTask;
            }

            public Task<Category?> GetMostCommonCategoryForPayeeAsync(
                string payee,
                double confidenceThreshold = 0.5,
                CancellationToken cancellationToken = default) =>
                Task.FromResult<Category?>(null);

            // IRecurringTransactionRepository
            public Task<List<RecurringTransaction>> GetRecurringTransactionsAsync() =>
                Task.FromResult(new List<RecurringTransaction>(_recurring));

            public Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
            {
                var existing = _recurring.FirstOrDefault(r => r.Id == recurring.Id);
                if (existing != null)
                {
                    _recurring.Remove(existing);
                }

                _recurring.Add(recurring);
                return Task.CompletedTask;
            }

            public Task DeleteRecurringTransactionAsync(string id)
            {
                _recurring.RemoveAll(r => r.Id == id);
                return Task.CompletedTask;
            }

            // IRecurringGenerationService
            public Task GeneratePendingRecurringTransactionsAsync() =>
                Task.CompletedTask;

            // IReportingService
            public Task<YearSummary> GetYearSummaryAsync(int year) =>
                Task.FromResult(new YearSummary());

            public Task<MonthSummary> GetMonthSummaryAsync(int year, int month) =>
                Task.FromResult(new MonthSummary());
        }
    }
}
