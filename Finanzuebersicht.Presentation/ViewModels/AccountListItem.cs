using Finanzuebersicht.Models;

namespace Finanzuebersicht.ViewModels;

public class AccountListItem(Account account, decimal saldo)
{
    public Account Account { get; } = account;
    public decimal Saldo { get; } = saldo;

    public string Name => Account.Name;
    public AccountType Type => Account.Type;
    public bool IsSystemAccount => Account.IsSystemAccount;
    public bool IsArchived => Account.IsArchived;
    public bool CanDelete => Account.CanDelete;
    public bool CanArchive => Account.CanArchive;
}
