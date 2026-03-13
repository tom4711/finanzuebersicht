namespace Finanzuebersicht.Models;

public class TransactionGroup(DateTime datum, IEnumerable<Transaction> transaktionen) : List<Transaction>(transaktionen)
{
    public DateTime Datum { get; } = datum;
    public string DatumFormatiert { get; } = datum.ToString("dd. MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
}
