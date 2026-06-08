using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class LoadTransactionsMonthUseCase(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IAccountRepository accountRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<TransactionsMonthData> ExecuteAsync(
        DateTime aktuellerMonat,
        string? accountId = null,
        CancellationToken cancellationToken = default)
    {
        var von = aktuellerMonat;
        var bis = aktuellerMonat.AddMonths(1).AddDays(-1);

        var liste = await _transactionRepository.GetTransactionsAsync(von, bis);
        if (accountId != null)
            liste = liste.Where(t => t.AccountId == accountId).ToList();

        var gruppen = liste
            .GroupBy(t => t.Datum.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new TransactionGroup(g.Key, g.OrderByDescending(t => t.Datum)))
            .ToList();

        var categories = await _categoryRepository.GetCategoriesAsync();
        var iconMap = categories.ToDictionary(c => c.Id, c => c.Icon ?? "📁");
        var accounts = await _accountRepository.GetAccountsAsync();
        var accountMap = accounts.ToDictionary(a => a.Id, a => a.Name);

        return new TransactionsMonthData
        {
            Gruppen = gruppen,
            IconMap = iconMap,
            AccountMap = accountMap
        };
    }
}

public class TransactionsMonthData
{
    public List<TransactionGroup> Gruppen { get; set; } = [];
    public Dictionary<string, string> IconMap { get; set; } = [];
    public Dictionary<string, string> AccountMap { get; set; } = [];
}