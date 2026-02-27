namespace Finanzuebersicht.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "💰";
    public string Color { get; set; } = "#007AFF";
    public TransactionType Typ { get; set; } = TransactionType.Ausgabe;
}
