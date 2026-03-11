using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum);
    Task SaveTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(string id);

    /// <summary>
    /// Finds the most common (non-Unkategorisiert) category for a given payee name.
    /// Uses case-insensitive matching and returns the category with highest frequency
    /// if it exceeds the confidence threshold (default 50%).
    /// </summary>
    /// <param name="payee">Payee name to search for</param>
    /// <param name="confidenceThreshold">Minimum ratio (0.0-1.0) of matching transactions that must share the same category. Default: 0.5 (50%)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most common category for this payee, or null if no match or threshold not met</returns>
    Task<Category?> GetMostCommonCategoryForPayeeAsync(
        string payee,
        double confidenceThreshold = 0.5,
        CancellationToken cancellationToken = default);
}
