namespace Finanzuebersicht.Models;

public class RecurringTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public decimal Betrag { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string KategorieId { get; set; } = string.Empty;
    public TransactionType Typ { get; set; } = TransactionType.Ausgabe;
    public DateTime Startdatum { get; set; } = DateTime.Today;
    public DateTime? Enddatum { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime LetzteAusfuehrung { get; set; }
}
