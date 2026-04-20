using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Services;

/// <summary>
/// Composite data service that coordinates multiple specialized stores.
/// Uses Dependency Injection to receive pre-configured stores from the DI container,
/// allowing proper logger propagation and testability.
///
/// - CategoryStore: Category persistence
/// - TransactionStore: Transaction persistence with smart categorization
/// - RecurringStore: Recurring transaction persistence
/// - ReportingService: Transaction aggregations
/// - RecurringGenerationService: Auto-generation of recurring transactions
/// </summary>
public class LocalDataService : IDataService, IDisposable
{
    private readonly CategoryStore _categoryStore;
    private readonly TransactionStore _transactionStore;
    private readonly RecurringStore _recurringStore;
    private readonly BudgetStore _budgetStore;
    private readonly SparZielStore _sparZielStore;
    private readonly ReportingService _reportingService;
    private readonly RecurringGenerationService _recurringGenerationService;

    /// <summary>
    /// Constructor for DI: receives pre-configured stores from the container.
    /// </summary>
    public LocalDataService(
        CategoryStore categoryStore,
        TransactionStore transactionStore,
        RecurringStore recurringStore,
        BudgetStore budgetStore,
        SparZielStore sparZielStore,
        IClock clock)
    {
        _categoryStore = categoryStore;
        _transactionStore = transactionStore;
        _recurringStore = recurringStore;
        _budgetStore = budgetStore;
        _sparZielStore = sparZielStore;
        _reportingService = new ReportingService(_transactionStore, _categoryStore);
        _recurringGenerationService = new RecurringGenerationService(_recurringStore, _transactionStore, clock);
    }

    /// <summary>
    /// Alternative constructor for manual instantiation (e.g., in tests).
    /// </summary>
    public LocalDataService(SettingsService? settings, IClock clock)
    {
        var defaultDataDir = AppPaths.GetDefaultDataDir();
        var customPath = settings?.Get("DataPath", "");
        var dataDir = string.IsNullOrWhiteSpace(customPath) ? defaultDataDir : customPath;

        _categoryStore = new CategoryStore(dataDir);
        _transactionStore = new TransactionStore(dataDir, categoryStore: _categoryStore);
        _recurringStore = new RecurringStore(dataDir);
        _budgetStore = new BudgetStore(dataDir);
        _sparZielStore = new SparZielStore(dataDir);
        _reportingService = new ReportingService(_transactionStore, _categoryStore);
        _recurringGenerationService = new RecurringGenerationService(_recurringStore, _transactionStore, clock);
    }

    #region ICategoryRepository delegation

    public async Task<List<Category>> GetCategoriesAsync()
        => await _categoryStore.GetCategoriesAsync();

    public async Task SaveCategoryAsync(Category category)
        => await _categoryStore.SaveCategoryAsync(category);

    public async Task DeleteCategoryAsync(string id)
        => await _categoryStore.DeleteCategoryAsync(id);

    public Task ReplaceAllCategoriesAsync(IEnumerable<Category> categories)
        => _categoryStore.ReplaceAllCategoriesAsync(categories);

    #endregion

    #region ITransactionRepository delegation

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
        => await _transactionStore.GetTransactionsAsync(vonDatum, bisDatum);

    public async Task SaveTransactionAsync(Transaction transaction)
        => await _transactionStore.SaveTransactionAsync(transaction);

    public async Task DeleteTransactionAsync(string id)
        => await _transactionStore.DeleteTransactionAsync(id);

    public Task ReplaceAllTransactionsAsync(IEnumerable<Transaction> transactions)
        => _transactionStore.ReplaceAllTransactionsAsync(transactions);

    public async Task<Category?> GetMostCommonCategoryForPayeeAsync(
        string payee,
        double confidenceThreshold = 0.5,
        CancellationToken cancellationToken = default)
        => await _transactionStore.GetMostCommonCategoryForPayeeAsync(payee, confidenceThreshold, cancellationToken);

    #endregion

    #region IRecurringTransactionRepository delegation

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync()
        => await _recurringStore.GetRecurringTransactionsAsync();

    public async Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
        => await _recurringStore.SaveRecurringTransactionAsync(recurring);

    public async Task DeleteRecurringTransactionAsync(string id)
        => await _recurringStore.DeleteRecurringTransactionAsync(id);

    public Task ReplaceAllRecurringTransactionsAsync(IEnumerable<RecurringTransaction> recurring)
        => _recurringStore.ReplaceAllRecurringTransactionsAsync(recurring);

    #endregion

    #region IReportingService delegation

    public async Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
        => await _reportingService.GetMonthSummaryAsync(year, month);

    public async Task<YearSummary> GetYearSummaryAsync(int year)
        => await _reportingService.GetYearSummaryAsync(year);

    #endregion

    #region IRecurringGenerationService delegation

    public async Task GeneratePendingRecurringTransactionsAsync()
        => await _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();

    #endregion

    #region IBudgetRepository delegation

    public Task<List<CategoryBudget>> GetBudgetsAsync() => _budgetStore.GetBudgetsAsync();
    public Task SaveBudgetAsync(CategoryBudget budget) => _budgetStore.SaveBudgetAsync(budget);
    public Task DeleteBudgetAsync(string id) => _budgetStore.DeleteBudgetAsync(id);
    public Task<CategoryBudget?> GetBudgetForCategoryAsync(string kategorieId, int year, int month)
        => _budgetStore.GetBudgetForCategoryAsync(kategorieId, year, month);
    public Task ReplaceAllBudgetsAsync(IEnumerable<CategoryBudget> budgets)
        => _budgetStore.ReplaceAllBudgetsAsync(budgets);

    #endregion

    #region ISparZielRepository delegation

    public Task<List<SparZiel>> GetSparZieleAsync() => _sparZielStore.GetSparZieleAsync();
    public Task SaveSparZielAsync(SparZiel sparZiel) => _sparZielStore.SaveSparZielAsync(sparZiel);
    public Task DeleteSparZielAsync(string id) => _sparZielStore.DeleteSparZielAsync(id);
    public Task ReplaceAllSparZieleAsync(IEnumerable<SparZiel> sparziele)
        => _sparZielStore.ReplaceAllSparZieleAsync(sparziele);

    #endregion

    public void Dispose()
    {
        // Intentionally left empty.
        // The injected stores are managed and disposed by the DI container.
    }
}
