using Finanzuebersicht.Models;
using Finanzuebersicht.Core.Services;

namespace Finanzuebersicht.Services;

/// <summary>
/// Composite data service that coordinates multiple specialized stores.
/// - CategoryStore: Category persistence
/// - TransactionStore: Transaction persistence with smart categorization
/// - RecurringStore: Recurring transaction persistence
/// - ReportingService: Transaction aggregations
/// - RecurringGenerationService: Auto-generation of recurring transactions
///
/// This design separates concerns while maintaining the unified IDataService interface.
/// </summary>
public class LocalDataService : IDataService, IDisposable
{
    private static readonly string DefaultDataDir = AppPaths.GetDefaultDataDir();

    private readonly string _dataDir;
    private readonly IClock _clock;

    private readonly CategoryStore _categoryStore;
    private readonly TransactionStore _transactionStore;
    private readonly RecurringStore _recurringStore;
    private readonly ReportingService _reportingService;
    private readonly RecurringGenerationService _recurringGenerationService;

    public LocalDataService() : this(null, new SystemClock(), null) { }

    public LocalDataService(SettingsService? settings, IClock clock, Microsoft.Extensions.Logging.ILogger<LocalDataService>? logger = null)
    {
        _clock = clock;
        var customPath = settings?.Get("DataPath", "");
        _dataDir = string.IsNullOrWhiteSpace(customPath) ? DefaultDataDir : customPath;

        // Initialize all specialized stores with shared data directory
        _categoryStore = new CategoryStore(_dataDir);
        _transactionStore = new TransactionStore(_dataDir);
        _recurringStore = new RecurringStore(_dataDir);
        _reportingService = new ReportingService(_transactionStore, _categoryStore);
        _recurringGenerationService = new RecurringGenerationService(_recurringStore, _transactionStore, _clock);
    }

    public string CurrentDataDir => _dataDir;

    #region ICategoryRepository delegation

    public async Task<List<Category>> GetCategoriesAsync()
        => await _categoryStore.GetCategoriesAsync();

    public async Task SaveCategoryAsync(Category category)
        => await _categoryStore.SaveCategoryAsync(category);

    public async Task DeleteCategoryAsync(string id)
        => await _categoryStore.DeleteCategoryAsync(id);

    #endregion

    #region ITransactionRepository delegation

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
        => await _transactionStore.GetTransactionsAsync(vonDatum, bisDatum);

    public async Task SaveTransactionAsync(Transaction transaction)
        => await _transactionStore.SaveTransactionAsync(transaction);

    public async Task DeleteTransactionAsync(string id)
        => await _transactionStore.DeleteTransactionAsync(id);

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

    public void Dispose()
    {
        _categoryStore.Dispose();
        _transactionStore.Dispose();
        _recurringStore.Dispose();
    }
}
