namespace Finanzuebersicht.Models;

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; } = AccountType.Girokonto;
    public string? SystemKey { get; set; }
    public bool IsArchived { get; set; }

    public bool IsSystemAccount => !string.IsNullOrWhiteSpace(SystemKey);
    public bool CanDelete => !IsSystemAccount;
    public bool CanArchive => !IsSystemAccount;
}
