using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Plugin interface for transaction categorization strategies.
/// Implementations attempt to assign a category to a transaction based on various heuristics.
/// </summary>
public interface ICategorizationStrategy
{
    /// <summary>
    /// Attempts to categorize a transaction.
    /// </summary>
    /// <param name="dto">The transaction DTO from the import source</param>
    /// <param name="availableCategories">All available categories to choose from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A matched Category, or null if this strategy cannot categorize the transaction</returns>
    Task<Category?> TryCategorizAsync(
        TransactionDto dto,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Priority order for strategy execution (lower values run first).
    /// Strategies run in ascending order; the first to return a non-null Category wins.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Human-readable name of the strategy (for logging/debugging).
    /// </summary>
    string Name { get; }
}
