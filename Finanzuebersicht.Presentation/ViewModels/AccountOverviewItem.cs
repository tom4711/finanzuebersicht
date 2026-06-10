namespace Finanzuebersicht.ViewModels;

public class AccountOverviewItem
{
    public required string AccountId { get; init; }
    public required string Name { get; init; }
    public decimal Saldo { get; init; }
    public decimal AnteilProzent { get; init; }
}
