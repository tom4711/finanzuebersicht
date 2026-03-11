using System.Text.Json;
using System.Linq;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

/// <summary>
/// Lokale JSON-basierte Implementierung der Repository-Ports.
/// Unterstützt einen konfigurierbaren Speicherpfad (z.B. iCloud Drive).
/// </summary>
public class LocalDataService : ICategoryRepository, ITransactionRepository, IRecurringTransactionRepository, IDisposable
{
    private static readonly string DefaultDataDir = AppPaths.GetDefaultDataDir();

    private readonly string _dataDir;
    private string CategoriesFile => Path.Combine(_dataDir, "categories.json");
    private string TransactionsFile => Path.Combine(_dataDir, "transactions.json");
    private string RecurringFile => Path.Combine(_dataDir, "recurring.json");

    private readonly SemaphoreSlim _categoriesLock = new(1, 1);
    private readonly SemaphoreSlim _transactionsLock = new(1, 1);
    private readonly SemaphoreSlim _recurringLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalDataService() : this(null) { }

    public LocalDataService(SettingsService? settings)
    {
        var customPath = settings?.Get("DataPath", "");
        _dataDir = string.IsNullOrWhiteSpace(customPath) ? DefaultDataDir : customPath;
        Directory.CreateDirectory(_dataDir);
    }

    public string CurrentDataDir => _dataDir;

    #region Categories

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await _categoriesLock.WaitAsync();
        try { return await LoadAsync<Category>(CategoriesFile); }
        finally { _categoriesLock.Release(); }
    }

    public async Task SaveCategoryAsync(Category category)
    {
        await _categoriesLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Category>(CategoriesFile);
            var idx = items.FindIndex(c => c.Id == category.Id);
            if (idx >= 0)
                items[idx] = category;
            else
                items.Add(category);
            await SaveAsync(CategoriesFile, items);
        }
        finally { _categoriesLock.Release(); }
    }

    public async Task DeleteCategoryAsync(string id)
    {
        await _categoriesLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Category>(CategoriesFile);
            items.RemoveAll(c => c.Id == id);
            await SaveAsync(CategoriesFile, items);
        }
        finally { _categoriesLock.Release(); }
    }

    #endregion

    #region Transactions

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
    {
        await _transactionsLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Transaction>(TransactionsFile);
            return items
                .Where(t => t.Datum >= vonDatum && t.Datum <= bisDatum)
                .OrderByDescending(t => t.Datum)
                .ToList();
        }
        finally { _transactionsLock.Release(); }
    }

    public async Task SaveTransactionAsync(Transaction transaction)
    {
        await _transactionsLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Transaction>(TransactionsFile);
            var idx = items.FindIndex(t => t.Id == transaction.Id);
            if (idx >= 0)
                items[idx] = transaction;
            else
                items.Add(transaction);
            await SaveAsync(TransactionsFile, items);
        }
        finally { _transactionsLock.Release(); }
    }

    public async Task DeleteTransactionAsync(string id)
    {
        await _transactionsLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Transaction>(TransactionsFile);
            items.RemoveAll(t => t.Id == id);
            await SaveAsync(TransactionsFile, items);
        }
        finally { _transactionsLock.Release(); }
    }

    // Aggregation: month summary
    public async Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
    {
        var service = new ReportingService(this, this);
        return await service.GetMonthSummaryAsync(year, month);
    }

    // Aggregation: year summary (12 months + by category)
    public async Task<YearSummary> GetYearSummaryAsync(int year)
    {
        var service = new ReportingService(this, this);
        return await service.GetYearSummaryAsync(year);
    }

    #endregion

    #region Recurring Transactions

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync()
    {
        await _recurringLock.WaitAsync();
        try { return await LoadAsync<RecurringTransaction>(RecurringFile); }
        finally { _recurringLock.Release(); }
    }

    public async Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
    {
        await _recurringLock.WaitAsync();
        try
        {
            var items = await LoadAsync<RecurringTransaction>(RecurringFile);
            var idx = items.FindIndex(r => r.Id == recurring.Id);
            if (idx >= 0)
                items[idx] = recurring;
            else
                items.Add(recurring);
            await SaveAsync(RecurringFile, items);
        }
        finally { _recurringLock.Release(); }
    }

    public async Task DeleteRecurringTransactionAsync(string id)
    {
        await _recurringLock.WaitAsync();
        try
        {
            var items = await LoadAsync<RecurringTransaction>(RecurringFile);
            items.RemoveAll(r => r.Id == id);
            await SaveAsync(RecurringFile, items);
        }
        finally { _recurringLock.Release(); }
    }

    public async Task GeneratePendingRecurringTransactionsAsync()
    {
        var service = new RecurringGenerationService(this, this);
        await service.GeneratePendingRecurringTransactionsAsync();
    }

    #endregion

    #region JSON Helpers

    private static async Task<List<T>> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            return [];

        var json = await File.ReadAllTextAsync(path);
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Deserialisieren von {path}: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Finds the most common category for a given payee name (case-insensitive).
    /// Only considers non-Unkategorisiert categories.
    /// </summary>
    public async Task<Category?> GetMostCommonCategoryForPayeeAsync(
        string payee,
        double confidenceThreshold = 0.5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payee))
            return null;

        await _transactionsLock.WaitAsync(cancellationToken);
        try
        {
            var transactions = await LoadAsync<Transaction>(TransactionsFile);
            var categories = await LoadAsync<Category>(CategoriesFile);

            // Normalize payee for case-insensitive matching
            var normalizedPayee = payee.Trim();

            // Find transactions with matching payee (case-insensitive)
            var matchingTransactions = transactions
                .Where(t => !string.IsNullOrEmpty(t.Titel) &&
                           (t.Titel.Equals(normalizedPayee, StringComparison.OrdinalIgnoreCase) ||
                            t.Titel.Contains(normalizedPayee, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingTransactions.Count == 0)
                return null;

            // Count categories, excluding Unkategorisiert
            var categoryCounts = matchingTransactions
                .GroupBy(t => t.KategorieId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (categoryCounts.Count == 0)
                return null;

            var topCount = categoryCounts.First().Count;
            var totalCount = matchingTransactions.Count;
            var confidence = (double)topCount / totalCount;

            // Check confidence threshold
            if (confidence < confidenceThreshold)
                return null;

            var topCategoryId = categoryCounts.First().CategoryId;
            var topCategory = categories.FirstOrDefault(c => c.Id == topCategoryId);

            // Don't use Unkategorisiert category
            if (topCategory != null && 
                topCategory.SystemKey != "SysCat_Unkategorisiert" && 
                topCategory.Name != "Unkategorisiert")
            {
                return topCategory;
            }

            return null;
        }
        finally { _transactionsLock.Release(); }
    }

    private static async Task SaveAsync<T>(string path, List<T> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    #endregion

    public void Dispose()
    {
        _categoriesLock.Dispose();
        _transactionsLock.Dispose();
        _recurringLock.Dispose();
    }
}
