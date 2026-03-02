using System.Text.Json;
using System.Linq;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

/// <summary>
/// Lokale JSON-basierte Implementierung von IDataService.
/// Unterstützt einen konfigurierbaren Speicherpfad (z.B. iCloud Drive).
/// </summary>
public class LocalDataService : IDataService, IDisposable
{
    private static readonly string DefaultDataDir = AppPaths.GetDefaultDataDir();

    private static string GetDefaultDataDir() => AppPaths.GetDefaultDataDir();

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
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        var items = await GetTransactionsAsync(from, to);
        var expenditure = items.Where(t => t.Typ == TransactionType.Ausgabe).ToList();

        var categories = await GetCategoriesAsync();
        var catDict = categories.ToDictionary(c => c.Id);

        var monthSummary = new MonthSummary
        {
            Year = year,
            Month = month,
            Total = expenditure.Sum(t => t.Betrag),
            ByCategory = expenditure
                .GroupBy(t => t.KategorieId)
                .Select(g =>
                {
                    catDict.TryGetValue(g.Key, out var cat);
                    return new CategorySummary
                    {
                        CategoryId = g.Key,
                        CategoryName = cat?.Name ?? string.Empty,
                        Total = g.Sum(t => t.Betrag),
                        Color = cat?.Color ?? "#007AFF",
                        Icon = cat?.Icon ?? "📁"
                    };
                })
                .ToList()
        };

        return monthSummary;
    }

    // Aggregation: year summary (12 months + by category)
    public async Task<YearSummary> GetYearSummaryAsync(int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        var items = await GetTransactionsAsync(from, to);
        var expenditure = items.Where(t => t.Typ == TransactionType.Ausgabe).ToList();

        var categories = await GetCategoriesAsync();
        var catDict = categories.ToDictionary(c => c.Id);

        var yearSummary = new YearSummary
        {
            Year = year,
            Total = expenditure.Sum(t => t.Betrag)
        };

        for (int m = 1; m <= 12; m++)
        {
            var monthItems = expenditure.Where(t => t.Datum.Month == m).ToList();
            var ms = new MonthSummary
            {
                Year = year,
                Month = m,
                Total = monthItems.Sum(t => t.Betrag),
                ByCategory = monthItems
                    .GroupBy(t => t.KategorieId)
                    .Select(g =>
                    {
                        catDict.TryGetValue(g.Key, out var cat);
                        return new CategorySummary
                        {
                            CategoryId = g.Key,
                            CategoryName = cat?.Name ?? string.Empty,
                            Total = g.Sum(t => t.Betrag),
                            Color = cat?.Color ?? "#007AFF",
                            Icon = cat?.Icon ?? "📁"
                        };
                    })
                    .ToList()
            };
            yearSummary.Months.Add(ms);
        }

        yearSummary.ByCategory = expenditure
            .GroupBy(t => t.KategorieId)
            .Select(g =>
            {
                catDict.TryGetValue(g.Key, out var cat);
                return new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = cat?.Name ?? string.Empty,
                    Total = g.Sum(t => t.Betrag),
                    Color = cat?.Color ?? "#007AFF",
                    Icon = cat?.Icon ?? "📁"
                };
            })
            .ToList();

        return yearSummary;
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
        var dauerauftraege = await GetRecurringTransactionsAsync();
        var heute = DateTime.Today;

        foreach (var da in dauerauftraege.Where(d => d.Aktiv))
        {
            if (da.Enddatum.HasValue && da.Enddatum.Value < heute)
                continue;

            var naechsterMonat = !da.LetzteAusfuehrung.HasValue
                ? da.Startdatum
                : new DateTime(da.LetzteAusfuehrung.Value.Year, da.LetzteAusfuehrung.Value.Month, 1).AddMonths(1);

            while (naechsterMonat <= heute)
            {
                var transaction = new Transaction
                {
                    Betrag = da.Betrag,
                    Titel = da.Titel,
                    Datum = naechsterMonat,
                    KategorieId = da.KategorieId,
                    Typ = da.Typ,
                    DauerauftragId = da.Id
                };

                await SaveTransactionAsync(transaction);

                da.LetzteAusfuehrung = naechsterMonat;
                naechsterMonat = naechsterMonat.AddMonths(1);
            }

            await SaveRecurringTransactionAsync(da);
        }
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
