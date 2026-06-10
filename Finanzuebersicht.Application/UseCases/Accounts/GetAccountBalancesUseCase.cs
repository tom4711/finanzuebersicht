using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Accounts;

public class GetAccountBalancesUseCase(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<IReadOnlyList<AccountBalanceSummary>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var accounts = await _accountRepository.GetAccountsAsync();
        var balances = accounts.ToDictionary(a => a.Id, _ => 0m);

        var transactions = await _transactionRepository.GetTransactionsAsync(
            new DateTime(1900, 1, 1),
            new DateTime(2100, 12, 31, 23, 59, 59));

        foreach (var transaction in transactions)
        {
            if (string.IsNullOrWhiteSpace(transaction.AccountId))
                continue;
            if (!balances.ContainsKey(transaction.AccountId))
                continue;

            balances[transaction.AccountId] += transaction.Typ == TransactionType.Einnahme
                ? Math.Abs(transaction.Betrag)
                : -Math.Abs(transaction.Betrag);
        }

        return accounts
            .Select(a =>
            {
                var transactionBalance = balances[a.Id];
                return new AccountBalanceSummary
                {
                    AccountId = a.Id,
                    AccountName = a.Name,
                    IsArchived = a.IsArchived,
                    OpeningBalance = a.OpeningBalance,
                    TransactionBalance = transactionBalance,
                    Saldo = a.OpeningBalance + transactionBalance
                };
            })
            .OrderBy(a => a.IsArchived)
            .ThenBy(a => a.AccountName)
            .ToList();
    }
}

public class AccountBalanceSummary
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TransactionBalance { get; set; }
    public decimal Saldo { get; set; }
}
