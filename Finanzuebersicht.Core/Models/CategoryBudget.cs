namespace Finanzuebersicht.Models;

public class CategoryBudget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string KategorieId { get; set; } = string.Empty;
    public decimal Betrag { get; set; }
    // null = applies to all months/years (default budget)
    public int? Monat { get; set; }
    public int? Jahr { get; set; }
}
