using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

/// <summary>
/// Orchestrates transaction categorization using pluggable strategies.
/// Strategies are executed in priority order (ascending); the first to return a Category wins.
/// Falls back to "Unkategorisiert" (Uncategorized) category if no strategy matches.
/// </summary>
public class CategorizationService(
    IEnumerable<ICategorizationStrategy> strategies,
    ILogger<CategorizationService>? logger = null)
{
    private readonly IEnumerable<ICategorizationStrategy> _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    private readonly ILogger<CategorizationService>? _logger = logger;

    /// <summary>
    /// Categorize a transaction using available strategies.
    /// Returns the first matched category, or the "Unkategorisiert" fallback if no strategy matches.
    /// </summary>
    public async Task<Category> CategorizAsync(
        TransactionDto dto,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ArgumentNullException.ThrowIfNull(availableCategories);

        var categories = availableCategories.ToList();
        var uncategorizedCategory = categories.FirstOrDefault(c => c.SystemKey == Finanzuebersicht.Constants.SystemCategoryKeys.Unkategorisiert)
            ?? categories.FirstOrDefault(c => c.Name == "Unkategorisiert");

        // Execute strategies in priority order
        var sortedStrategies = _strategies.OrderBy(s => s.Priority).ToList();

        foreach (var strategy in sortedStrategies)
        {
            try
            {
                var matchedCategory = await strategy.TryCategorizAsync(dto, categories, cancellationToken).ConfigureAwait(false);
                if (matchedCategory != null)
                {
                    _logger?.LogInformation(
                        "Transaction '{Title}' auto-categorized by {StrategyName} → {CategoryName}",
                        dto.Verwendungszweck ?? "(no reference)",
                        strategy.Name,
                        matchedCategory.Name);
                    try { FileLogger.Append("CategorizationService", $"Auto-categorized by {strategy.Name} → {matchedCategory.Name}"); } catch { }
                    return matchedCategory;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "Strategy {StrategyName} threw exception during categorization",
                    strategy.Name);
                try { FileLogger.Append("CategorizationService", $"Strategy {strategy.Name} error: {ex.Message}"); } catch { }
            }
        }

        // Fallback to Unkategorisiert
        if (uncategorizedCategory != null)
        {
            _logger?.LogInformation(
                "Transaction '{Title}' fell back to Unkategorisiert (no strategy matched)",
                dto.Verwendungszweck ?? "(no reference)");
            try { FileLogger.Append("CategorizationService", "No strategy matched - using Unkategorisiert fallback"); } catch { }
            return uncategorizedCategory;
        }

        throw new InvalidOperationException("Unkategorisiert category not found in available categories");
    }
}
