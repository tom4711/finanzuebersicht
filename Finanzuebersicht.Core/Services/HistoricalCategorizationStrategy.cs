using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Categorizes transactions based on historical categorization patterns.
/// Finds transactions with similar payees and uses the most common (non-Unkategorisiert) category
/// if confidence threshold is met.
/// </summary>
public class HistoricalCategorizationStrategy : ICategorizationStrategy
{
    private readonly ITransactionRepository _txRepo;
    private readonly ILogger<HistoricalCategorizationStrategy>? _logger;
    private readonly double _confidenceThreshold;

    public int Priority => 20;  // Run after keyword matching
    public string Name => "Historical Category Matching";

    public HistoricalCategorizationStrategy(
        ITransactionRepository txRepo,
        ILogger<HistoricalCategorizationStrategy>? logger = null,
        double confidenceThreshold = 0.5)
    {
        _txRepo = txRepo ?? throw new ArgumentNullException(nameof(txRepo));
        _logger = logger;
        _confidenceThreshold = Math.Max(0.0, Math.Min(1.0, confidenceThreshold));
    }

    public async Task<Category?> TryCategorizAsync(
        TransactionDto dto,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default)
    {
        var payee = GetPayee(dto);
        if (string.IsNullOrEmpty(payee))
        {
            return null;
        }

        try
        {
            var matchedCategory = await _txRepo.GetMostCommonCategoryForPayeeAsync(
                payee,
                _confidenceThreshold,
                cancellationToken).ConfigureAwait(false);

            return matchedCategory;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error looking up historical category for payee '{Payee}'", payee);
            try { FileLogger.Append("HistoricalCategorizationStrategy", $"Error for payee {payee}: {ex.Message}"); } catch { }
            return null;
        }
    }

    private string GetPayee(TransactionDto dto)
    {
        // Prefer payee (recipient), fall back to payer
        return !string.IsNullOrEmpty(dto.Zahlungsempfaenger)
            ? dto.Zahlungsempfaenger.Trim()
            : !string.IsNullOrEmpty(dto.Zahlungspflichtige)
                ? dto.Zahlungspflichtige.Trim()
                : null;
    }
}
