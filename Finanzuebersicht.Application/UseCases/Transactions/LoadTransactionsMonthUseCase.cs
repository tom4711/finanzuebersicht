using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class LoadTransactionsMonthUseCase(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<TransactionsMonthData> ExecuteAsync(DateTime aktuellerMonat)
    {
        var von = aktuellerMonat;
        var bis = aktuellerMonat.AddMonths(1).AddDays(-1);

        var liste = await _transactionRepository.GetTransactionsAsync(von, bis);
        var gruppen = liste
            .GroupBy(t => t.Datum.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new TransactionGroup(g.Key, g.OrderByDescending(t => t.Datum)))
            .ToList();

        var categories = await _categoryRepository.GetCategoriesAsync();
        var iconMap = categories.ToDictionary(c => c.Id, c => c.Icon ?? "📁");

        return new TransactionsMonthData
        {
            Gruppen = gruppen,
            IconMap = iconMap
        };
    }
}

public class TransactionsMonthData
{
    public List<TransactionGroup> Gruppen { get; set; } = [];
    public Dictionary<string, string> IconMap { get; set; } = [];
}