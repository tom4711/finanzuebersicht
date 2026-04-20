using Xunit;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Application.UseCases.Categories;

namespace Finanzuebersicht.Tests.Services
{
    /// <summary>
    /// Integration tests for critical data consistency flows.
    /// Tests exercise production code paths through actual UseCases and Services.
    /// </summary>
    public class DataConsistencyFlowIntegrationTests
    {
        [Fact]
        public async Task DeleteCategoryUseCase_WithTransactions_ReassignsToFallback()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            
            var groceriesCategory = new Category
            {
                Id = "cat-groceries",
                Name = "Groceries",
                Icon = "🛒",
                Color = "#34C759",
                Typ = TransactionType.Ausgabe
            };
            var sonstiges = new Category
            {
                Id = "cat-sonstiges",
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = Finanzuebersicht.Core.Constants.SystemCategoryKeys.Sonstiges
            };

            await dataService.SaveCategoryAsync(groceriesCategory);
            await dataService.SaveCategoryAsync(sonstiges);

            var txn1 = new Transaction
            {
                Id = "txn1",
                Titel = "Supermarket",
                Betrag = 50m,
                Datum = DateTime.Today,
                KategorieId = "cat-groceries",
                Typ = TransactionType.Ausgabe
            };
            var txn2 = new Transaction
            {
                Id = "txn2",
                Titel = "Farm Market",
                Betrag = 30m,
                Datum = DateTime.Today,
                KategorieId = "cat-groceries",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveTransactionAsync(txn1);
            await dataService.SaveTransactionAsync(txn2);

            var useCase = new DeleteCategoryUseCase(dataService, dataService, dataService);

            // Act
            await useCase.ExecuteAsync("cat-groceries");

            // Assert - verify transactions were reassigned to fallback
            var allTransactions = await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1));

            var txn1After = allTransactions.FirstOrDefault(t => t.Id == "txn1");
            var txn2After = allTransactions.FirstOrDefault(t => t.Id == "txn2");

            Assert.NotNull(txn1After);
            Assert.NotNull(txn2After);
            Assert.Equal("cat-sonstiges", txn1After.KategorieId);
            Assert.Equal("cat-sonstiges", txn2After.KategorieId);

            // Verify category was deleted
            var categories = await dataService.GetCategoriesAsync();
            var deletedCat = categories.FirstOrDefault(c => c.Id == "cat-groceries");
            Assert.Null(deletedCat);
        }

        [Fact]
        public async Task DeleteCategoryUseCase_WithRecurringTransactions_ReassignsToFallback()
        {
            // Arrange
            var dataService = new InMemoryDataService();
            
            var utilitiesCategory = new Category
            {
                Id = "cat-utilities",
                Name = "Utilities",
                Icon = "⚡",
                Color = "#FF9500",
                Typ = TransactionType.Ausgabe
            };
            var sonstiges = new Category
            {
                Id = "cat-sonstiges",
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = Finanzuebersicht.Core.Constants.SystemCategoryKeys.Sonstiges
            };

            await dataService.SaveCategoryAsync(utilitiesCategory);
            await dataService.SaveCategoryAsync(sonstiges);

            var recurring = new RecurringTransaction
            {
                Id = "recurring1",
                Titel = "Monthly Bill",
                Betrag = 100m,
                KategorieId = "cat-utilities",
                Typ = TransactionType.Ausgabe,
                Startdatum = DateTime.Today.AddMonths(-3),
                Aktiv = true
            };

            await dataService.SaveRecurringTransactionAsync(recurring);

            var useCase = new DeleteCategoryUseCase(dataService, dataService, dataService);

            // Act
            await useCase.ExecuteAsync("cat-utilities");

            // Assert - verify recurring was reassigned to fallback
            var recurringAfter = await dataService.GetRecurringTransactionsAsync();
            var recurringAfterDelete = recurringAfter.FirstOrDefault(r => r.Id == "recurring1");

            Assert.NotNull(recurringAfterDelete);
            Assert.Equal("cat-sonstiges", recurringAfterDelete.KategorieId);

            var categories = await dataService.GetCategoriesAsync();
            var deletedCat = categories.FirstOrDefault(c => c.Id == "cat-utilities");
            Assert.Null(deletedCat);
        }

        [Fact]
        public async Task DeleteCategoryUseCase_WithoutFallback_CreatesSystemFallback()
        {
            // Arrange - only have the category to delete, no fallback exists
            var dataService = new InMemoryDataService();
            
            var onlyCategory = new Category
            {
                Id = "cat-only",
                Name = "OnlyOne",
                Icon = "🏠",
                Color = "#000000",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveCategoryAsync(onlyCategory);

            var txn = new Transaction
            {
                Id = "txn-orphan",
                Titel = "Test",
                Betrag = 100m,
                Datum = DateTime.Today,
                KategorieId = "cat-only",
                Typ = TransactionType.Ausgabe
            };

            await dataService.SaveTransactionAsync(txn);

            var useCase = new DeleteCategoryUseCase(dataService, dataService, dataService);

            // Act - UseCase should create fallback if needed
            await useCase.ExecuteAsync("cat-only");

            // Assert - fallback was created
            var categories = await dataService.GetCategoriesAsync();
            var fallback = categories.FirstOrDefault(c => c.SystemKey == Finanzuebersicht.Core.Constants.SystemCategoryKeys.Sonstiges);
            
            Assert.NotNull(fallback);
            
            var txnAfter = (await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1)))
                .FirstOrDefault(t => t.Id == "txn-orphan");
            
            Assert.NotNull(txnAfter);
            Assert.Equal(fallback.Id, txnAfter.KategorieId);
        }

        [Fact]
        public async Task DeleteCategoryUseCase_MixedData_HandlesBothTransactionsAndRecurring()
        {
            // Arrange - category used by both transactions and recurring
            var dataService = new InMemoryDataService();
            
            var entertainmentCategory = new Category
            {
                Id = "cat-entertainment",
                Name = "Entertainment",
                Icon = "🎬",
                Color = "#AF52DE",
                Typ = TransactionType.Ausgabe
            };
            var sonstiges = new Category
            {
                Id = "cat-sonstiges",
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = Finanzuebersicht.Core.Constants.SystemCategoryKeys.Sonstiges
            };

            await dataService.SaveCategoryAsync(entertainmentCategory);
            await dataService.SaveCategoryAsync(sonstiges);

            // Add transaction
            var txn = new Transaction
            {
                Id = "txn-movie",
                Titel = "Movie",
                Betrag = 25m,
                Datum = DateTime.Today,
                KategorieId = "cat-entertainment",
                Typ = TransactionType.Ausgabe
            };
            await dataService.SaveTransactionAsync(txn);

            // Add recurring
            var recurring = new RecurringTransaction
            {
                Id = "rec-cinema",
                Titel = "Monthly Cinema",
                Betrag = 40m,
                KategorieId = "cat-entertainment",
                Typ = TransactionType.Ausgabe,
                Startdatum = DateTime.Today.AddMonths(-1),
                Aktiv = true
            };
            await dataService.SaveRecurringTransactionAsync(recurring);

            var useCase = new DeleteCategoryUseCase(dataService, dataService, dataService);

            // Act
            await useCase.ExecuteAsync("cat-entertainment");

            // Assert - both should be reassigned
            var txnAfter = (await dataService.GetTransactionsAsync(
                DateTime.Today.AddMonths(-12),
                DateTime.Today.AddDays(1)))
                .FirstOrDefault(t => t.Id == "txn-movie");
            
            var recAfter = (await dataService.GetRecurringTransactionsAsync())
                .FirstOrDefault(r => r.Id == "rec-cinema");

            Assert.NotNull(txnAfter);
            Assert.NotNull(recAfter);
            Assert.Equal("cat-sonstiges", txnAfter.KategorieId);
            Assert.Equal("cat-sonstiges", recAfter.KategorieId);
        }

        /// <summary>
        /// In-memory implementation of IDataService for testing.
        /// Provides isolated test environment without file I/O.
        /// </summary>
        private class InMemoryDataService : IDataService
        {
            private readonly List<Category> _categories = [];
            private readonly List<Transaction> _transactions = [];
            private readonly List<RecurringTransaction> _recurring = [];
            private readonly List<CategoryBudget> _budgets = [];
            private readonly List<SparZiel> _sparziele = [];

            // ICategoryRepository
            public Task<List<Category>> GetCategoriesAsync() =>
                Task.FromResult(new List<Category>(_categories));

            public Task SaveCategoryAsync(Category category)
            {
                var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
                if (existing != null)
                    _categories.Remove(existing);

                _categories.Add(category);
                return Task.CompletedTask;
            }

            public Task DeleteCategoryAsync(string id)
            {
                _categories.RemoveAll(c => c.Id == id);
                return Task.CompletedTask;
            }

            public Task ReplaceAllCategoriesAsync(IEnumerable<Category> categories)
            {
                _categories.Clear();
                _categories.AddRange(categories);
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
                    _transactions.Remove(existing);

                _transactions.Add(transaction);
                return Task.CompletedTask;
            }

            public Task DeleteTransactionAsync(string id)
            {
                _transactions.RemoveAll(t => t.Id == id);
                return Task.CompletedTask;
            }

            public Task ReplaceAllTransactionsAsync(IEnumerable<Transaction> transactions)
            {
                _transactions.Clear();
                _transactions.AddRange(transactions);
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
                    _recurring.Remove(existing);

                _recurring.Add(recurring);
                return Task.CompletedTask;
            }

            public Task DeleteRecurringTransactionAsync(string id)
            {
                _recurring.RemoveAll(r => r.Id == id);
                return Task.CompletedTask;
            }

            public Task ReplaceAllRecurringTransactionsAsync(IEnumerable<RecurringTransaction> recurring)
            {
                _recurring.Clear();
                _recurring.AddRange(recurring);
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

            // IBudgetRepository
            public Task<List<CategoryBudget>> GetBudgetsAsync() => Task.FromResult(new List<CategoryBudget>(_budgets));
            public Task SaveBudgetAsync(CategoryBudget budget)
            {
                var idx = _budgets.FindIndex(b => b.Id == budget.Id);
                if (idx >= 0) _budgets[idx] = budget; else _budgets.Add(budget);
                return Task.CompletedTask;
            }
            public Task DeleteBudgetAsync(string id) { _budgets.RemoveAll(b => b.Id == id); return Task.CompletedTask; }
            public Task<CategoryBudget?> GetBudgetForCategoryAsync(string kategorieId, int year, int month) => Task.FromResult<CategoryBudget?>(null);
            public Task ReplaceAllBudgetsAsync(IEnumerable<CategoryBudget> budgets) { _budgets.Clear(); _budgets.AddRange(budgets); return Task.CompletedTask; }

            // ISparZielRepository
            public Task<List<SparZiel>> GetSparZieleAsync() => Task.FromResult(new List<SparZiel>(_sparziele));
            public Task SaveSparZielAsync(SparZiel sparZiel)
            {
                var idx = _sparziele.FindIndex(s => s.Id == sparZiel.Id);
                if (idx >= 0) _sparziele[idx] = sparZiel; else _sparziele.Add(sparZiel);
                return Task.CompletedTask;
            }
            public Task DeleteSparZielAsync(string id) { _sparziele.RemoveAll(s => s.Id == id); return Task.CompletedTask; }
            public Task ReplaceAllSparZieleAsync(IEnumerable<SparZiel> sparziele) { _sparziele.Clear(); _sparziele.AddRange(sparziele); return Task.CompletedTask; }
        }
    }
}
