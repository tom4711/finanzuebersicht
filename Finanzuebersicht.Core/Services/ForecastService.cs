using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class ForecastService(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IBudgetRepository budgetRepository) : IForecastService
{
    public async Task<ForecastResult> GetMovingAverageAsync(int year, int month, int lookbackMonths = 3)
    {
        if (lookbackMonths < 1)
            throw new ArgumentOutOfRangeException(nameof(lookbackMonths), "lookbackMonths must be >= 1");

        var categories = await categoryRepository.GetCategoriesAsync();
        var catDict = categories.ToDictionary(c => c.Id);

        // Collect transactions from the last N months before the target month
        var allTransactions = new List<Transaction>();
        for (int i = lookbackMonths; i >= 1; i--)
        {
            var refDate = new DateTime(year, month, 1).AddMonths(-i);
            var from = new DateTime(refDate.Year, refDate.Month, 1);
            var to = from.AddMonths(1).AddTicks(-1);
            var monthTx = await transactionRepository.GetTransactionsAsync(from, to);
            allTransactions.AddRange(monthTx);
        }

        var expenditure = allTransactions.Where(t => t.Typ == TransactionType.Ausgabe).ToList();

        // Moving average per category
        var byCategory = expenditure
            .GroupBy(t => t.KategorieId)
            .Select(g =>
            {
                catDict.TryGetValue(g.Key, out var cat);
                var avgTotal = g.Sum(t => Math.Abs(t.Betrag)) / lookbackMonths;
                return new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = cat?.Name ?? string.Empty,
                    Total = avgTotal,
                    Color = cat?.Color ?? "#007AFF",
                    Icon = cat?.Icon ?? "📁"
                };
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        // Enrich with budgets using the repository's standard priority:
        // specific year+month > month-only > default
        foreach (var cs in byCategory)
        {
            var budget = await budgetRepository.GetBudgetForCategoryAsync(cs.CategoryId, year, month);
            if (budget != null)
                cs.BudgetBetrag = budget.Betrag;
        }

        return new ForecastResult
        {
            Year = year,
            Month = month,
            ForecastedTotal = byCategory.Sum(c => c.Total),
            ByCategory = byCategory,
            LookbackMonths = lookbackMonths
        };
    }
}
