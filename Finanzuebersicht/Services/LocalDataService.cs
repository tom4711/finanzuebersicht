using System.Text.Json;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

/// <summary>
/// Lokale JSON-basierte Implementierung von IDataService zum Testen ohne CloudKit.
/// </summary>
public class LocalDataService : IDataService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Finanzuebersicht");

    private static readonly string CategoriesFile = Path.Combine(DataDir, "categories.json");
    private static readonly string TransactionsFile = Path.Combine(DataDir, "transactions.json");
    private static readonly string RecurringFile = Path.Combine(DataDir, "recurring.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalDataService()
    {
        Directory.CreateDirectory(DataDir);
    }

    #region Categories

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await LoadAsync<Category>(CategoriesFile);
    }

    public async Task SaveCategoryAsync(Category category)
    {
        var items = await LoadAsync<Category>(CategoriesFile);
        var idx = items.FindIndex(c => c.Id == category.Id);
        if (idx >= 0)
            items[idx] = category;
        else
            items.Add(category);
        await SaveAsync(CategoriesFile, items);
    }

    public async Task DeleteCategoryAsync(string id)
    {
        var items = await LoadAsync<Category>(CategoriesFile);
        items.RemoveAll(c => c.Id == id);
        await SaveAsync(CategoriesFile, items);
    }

    #endregion

    #region Transactions

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
    {
        var items = await LoadAsync<Transaction>(TransactionsFile);
        return items
            .Where(t => t.Datum >= vonDatum && t.Datum <= bisDatum)
            .OrderByDescending(t => t.Datum)
            .ToList();
    }

    public async Task SaveTransactionAsync(Transaction transaction)
    {
        var items = await LoadAsync<Transaction>(TransactionsFile);
        var idx = items.FindIndex(t => t.Id == transaction.Id);
        if (idx >= 0)
            items[idx] = transaction;
        else
            items.Add(transaction);
        await SaveAsync(TransactionsFile, items);
    }

    public async Task DeleteTransactionAsync(string id)
    {
        var items = await LoadAsync<Transaction>(TransactionsFile);
        items.RemoveAll(t => t.Id == id);
        await SaveAsync(TransactionsFile, items);
    }

    #endregion

    #region Recurring Transactions

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync()
    {
        return await LoadAsync<RecurringTransaction>(RecurringFile);
    }

    public async Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
    {
        var items = await LoadAsync<RecurringTransaction>(RecurringFile);
        var idx = items.FindIndex(r => r.Id == recurring.Id);
        if (idx >= 0)
            items[idx] = recurring;
        else
            items.Add(recurring);
        await SaveAsync(RecurringFile, items);
    }

    public async Task DeleteRecurringTransactionAsync(string id)
    {
        var items = await LoadAsync<RecurringTransaction>(RecurringFile);
        items.RemoveAll(r => r.Id == id);
        await SaveAsync(RecurringFile, items);
    }

    public async Task GeneratePendingRecurringTransactionsAsync()
    {
        var dauerauftraege = await GetRecurringTransactionsAsync();
        var heute = DateTime.Today;

        foreach (var da in dauerauftraege.Where(d => d.Aktiv))
        {
            if (da.Enddatum.HasValue && da.Enddatum.Value < heute)
                continue;

            var naechsterMonat = da.LetzteAusfuehrung == default
                ? new DateTime(da.Startdatum.Year, da.Startdatum.Month, 1)
                : da.LetzteAusfuehrung.AddMonths(1);
            naechsterMonat = new DateTime(naechsterMonat.Year, naechsterMonat.Month, 1);

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
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
    }

    private static async Task SaveAsync<T>(string path, List<T> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    #endregion
}
