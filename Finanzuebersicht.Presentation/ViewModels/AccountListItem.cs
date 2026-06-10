using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels;

public class AccountListItem
{
    public AccountListItem(Account account, AccountBalanceSummary? summary = null)
    {
        Account = account;
        var balance = summary ?? new AccountBalanceSummary { AccountId = account.Id };
        Saldo = balance.Saldo;
        OpeningBalance = balance.OpeningBalance;
        TransactionBalance = balance.TransactionBalance;
    }

    public Account Account { get; }
    public decimal Saldo { get; }
    public decimal OpeningBalance { get; }
    public decimal TransactionBalance { get; }
    public string? BalanceBreakdownText { get; init; }

    public string Name => Account.Name;
    public AccountType Type => Account.Type;
    public bool IsSystemAccount => Account.IsSystemAccount;
    public bool IsArchived => Account.IsArchived;
    public bool CanDelete => Account.CanDelete;
    public bool CanArchive => Account.CanArchive;
    public bool ShowBalanceBreakdown => !string.IsNullOrWhiteSpace(BalanceBreakdownText);
}
