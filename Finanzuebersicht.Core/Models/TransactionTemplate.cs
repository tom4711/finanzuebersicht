namespace Finanzuebersicht.Models;

public class TransactionTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Titel { get; set; } = string.Empty;
    public decimal Betrag { get; set; }
    public string KategorieId { get; set; } = string.Empty;
    public TransactionType Typ { get; set; } = TransactionType.Ausgabe;
    public string Verwendungszweck { get; set; } = string.Empty;
    public DateTime? LastUsedAt { get; set; }
    public int UseCount { get; set; }
}
