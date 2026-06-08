using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadDashboardYearUseCase(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<DashboardYearData> ExecuteAsync(int year, string? accountId = null, CancellationToken cancellationToken = default)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        var categories = await _categoryRepository.GetCategoriesAsync();
        var categoryDict = categories.ToDictionary(c => c.Id);

        var transactions = await _transactionRepository.GetTransactionsAsync(from, to);
        transactions = transactions
            .Where(t => t.Typ == TransactionType.Ausgabe && !t.IsTransfer)
            .ToList();

        if (!string.IsNullOrWhiteSpace(accountId))
            transactions = transactions.Where(t => t.AccountId == accountId).ToList();

        var total = transactions.Sum(t => Math.Abs(t.Betrag));
        var byCategory = transactions
            .GroupBy(t => t.KategorieId)
            .Select(g =>
            {
                categoryDict.TryGetValue(g.Key, out var category);
                return new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = category?.Name ?? string.Empty,
                    Total = g.Sum(t => Math.Abs(t.Betrag)),
                    Color = category?.Color ?? "#007AFF",
                    Icon = category?.Icon ?? "📁"
                };
            })
            .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
            .OrderByDescending(c => c.Total)
            .ToList();

        var months = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var monthItems = transactions.Where(t => t.Datum.Month == month).ToList();
                var monthTotal = monthItems.Sum(t => Math.Abs(t.Betrag));
                var monthByCategory = monthItems
                    .GroupBy(t => t.KategorieId)
                    .Select(g =>
                    {
                        categoryDict.TryGetValue(g.Key, out var category);
                        return new CategorySummary
                        {
                            CategoryId = g.Key,
                            CategoryName = category?.Name ?? string.Empty,
                            Total = g.Sum(t => Math.Abs(t.Betrag)),
                            Color = category?.Color ?? "#007AFF",
                            Icon = category?.Icon ?? "📁"
                        };
                    })
                    .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
                    .ToList();

                return new MonthSummary
                {
                    Year = year,
                    Month = month,
                    Total = monthTotal,
                    ByCategory = monthByCategory
                };
            })
            .ToList();

        if (total > 0)
        {
            foreach (var cat in byCategory)
            {
                cat.PercentageAmount = cat.Total / total * 100;
            }
        }

        return new DashboardYearData
        {
            GesamtAusgaben = total,
            Monate = months,
            Kategorien = byCategory
        };
    }
}

public class DashboardYearData
{
    public decimal GesamtAusgaben { get; set; }
    public List<MonthSummary> Monate { get; set; } = [];
    public List<CategorySummary> Kategorien { get; set; } = [];
}