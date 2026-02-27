namespace Finanzuebersicht.Models;

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public decimal Betrag { get; set; }
    public string Titel { get; set; } = string.Empty;
    public DateTime Datum { get; set; } = DateTime.Today;
    public string KategorieId { get; set; } = string.Empty;
    public TransactionType Typ { get; set; } = TransactionType.Ausgabe;
    public string? DauerauftragId { get; set; }
}
