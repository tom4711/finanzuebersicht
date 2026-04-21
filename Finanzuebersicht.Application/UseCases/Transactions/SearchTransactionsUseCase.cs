using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public enum TransactionTypeFilter { Alle, Einnahme, Ausgabe }

public record SearchTransactionsQuery(
    string SearchText = "",
    string? KategorieId = null,
    TransactionTypeFilter Typ = TransactionTypeFilter.Alle,
    DateTime? VonDatum = null,
    DateTime? BisDatum = null
);

public class SearchTransactionsResult
{
    public List<TransactionGroup> Gruppen { get; set; } = [];
    public Dictionary<string, string> IconMap { get; set; } = [];
    public int TotalCount { get; set; }
}

public class SearchTransactionsUseCase(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<SearchTransactionsResult> ExecuteAsync(SearchTransactionsQuery query)
    {
        var von = query.VonDatum ?? DateTime.MinValue;
        var bis = query.BisDatum ?? DateTime.MaxValue;

        var alle = await _transactionRepository.GetTransactionsAsync(von, bis);

        var gefiltert = alle.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var text = query.SearchText.Trim();
            gefiltert = gefiltert.Where(t =>
                t.Titel.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                t.Verwendungszweck.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        if (query.KategorieId != null)
            gefiltert = gefiltert.Where(t => t.KategorieId == query.KategorieId);

        if (query.Typ == TransactionTypeFilter.Einnahme)
            gefiltert = gefiltert.Where(t => t.Typ == TransactionType.Einnahme);
        else if (query.Typ == TransactionTypeFilter.Ausgabe)
            gefiltert = gefiltert.Where(t => t.Typ == TransactionType.Ausgabe);

        var liste = gefiltert.OrderByDescending(t => t.Datum).ToList();

        var gruppen = liste
            .GroupBy(t => new DateTime(t.Datum.Year, t.Datum.Month, 1))
            .OrderByDescending(g => g.Key)
            .Select(g => new TransactionGroup(g.Key, g.OrderByDescending(t => t.Datum), isMonthGroup: true))
            .ToList();

        var categories = await _categoryRepository.GetCategoriesAsync();
        var iconMap = categories.ToDictionary(c => c.Id, c => c.Icon ?? "📁");

        return new SearchTransactionsResult
        {
            Gruppen = gruppen,
            IconMap = iconMap,
            TotalCount = liste.Count
        };
    }
}
