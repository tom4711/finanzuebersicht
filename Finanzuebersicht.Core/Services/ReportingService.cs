using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class ReportingService : IReportingService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ReportingService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        var items = await _transactionRepository.GetTransactionsAsync(from, to);
        var expenditure = items.Where(t => t.Typ == TransactionType.Ausgabe).ToList();

        var categories = await _categoryRepository.GetCategoriesAsync();
        var catDict = categories.ToDictionary(c => c.Id);

        return new MonthSummary
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
    }

    public async Task<YearSummary> GetYearSummaryAsync(int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31, 23, 59, 59);
        var items = await _transactionRepository.GetTransactionsAsync(from, to);
        var expenditure = items.Where(t => t.Typ == TransactionType.Ausgabe).ToList();

        var categories = await _categoryRepository.GetCategoriesAsync();
        var catDict = categories.ToDictionary(c => c.Id);

        var yearSummary = new YearSummary
        {
            Year = year,
            Total = expenditure.Sum(t => t.Betrag)
        };

        for (int m = 1; m <= 12; m++)
        {
            var monthItems = expenditure.Where(t => t.Datum.Month == m).ToList();
            var monthSummary = new MonthSummary
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

            yearSummary.Months.Add(monthSummary);
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
}
