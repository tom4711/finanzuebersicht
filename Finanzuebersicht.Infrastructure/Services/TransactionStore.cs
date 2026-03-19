using System.Linq;
using Finanzuebersicht.Models;
using Finanzuebersicht.Core.Constants;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Services;

/// <summary>
/// JSON-based adapter for Transaction repository operations.
/// Handles persistence of transactions with thread-safe CRUD operations
/// and smart category inference for payees.
/// </summary>
public class TransactionStore : JsonDataStoreBase, ITransactionRepository
{
    private string TransactionsFile => Path.Combine(DataDir, "transactions.json");
    private string CategoriesFile => Path.Combine(DataDir, "categories.json");

    public TransactionStore(string dataDir, ILogger<TransactionStore>? logger = null)
        : base(dataDir, logger)
    {
    }

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
    {
        await StoreLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Transaction>(TransactionsFile);
            return [..items
                .Where(t => t.Datum >= vonDatum && t.Datum <= bisDatum)
                .OrderByDescending(t => t.Datum)];
        }
        finally
        {
            StoreLock.Release();
        }
    }

    public async Task SaveTransactionAsync(Transaction transaction)
    {
        await StoreLock.WaitAsync();
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
        finally
        {
            StoreLock.Release();
        }
    }

    public async Task DeleteTransactionAsync(string id)
    {
        await StoreLock.WaitAsync();
        try
        {
            var items = await LoadAsync<Transaction>(TransactionsFile);
            items.RemoveAll(t => t.Id == id);
            await SaveAsync(TransactionsFile, items);
        }
        finally
        {
            StoreLock.Release();
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

        await StoreLock.WaitAsync(cancellationToken);
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
                topCategory.SystemKey != SystemCategoryKeys.Unkategorisiert &&
                topCategory.Name != "Unkategorisiert")
            {
                return topCategory;
            }

            return null;
        }
        finally
        {
            StoreLock.Release();
        }
    }
}
