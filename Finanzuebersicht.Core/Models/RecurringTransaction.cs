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
    public DateTime? LetzteAusfuehrung { get; set; }

    // Issue #48: Erweiterte Felder für wiederkehrende Zahlungen
    public RecurrenceInterval Interval { get; set; } = RecurrenceInterval.Monthly;
    // Intervall-Faktor: z.B. 1 = every month/week/quarter, 2 = every 2 months/weeks
    public int IntervalFactor { get; set; } = 1;
    // Reminder / Hinweis: Tage vor Fälligkeit
    public int ReminderDaysBefore { get; set; } = 0;
    // Ausnahmeregeln (Skip / Shift) für einzelne Instanzen
    public List<RecurringException> Exceptions { get; set; } = new();
}
