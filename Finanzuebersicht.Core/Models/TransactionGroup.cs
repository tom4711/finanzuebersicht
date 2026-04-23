namespace Finanzuebersicht.Models;

public class TransactionGroup(DateTime datum, IEnumerable<Transaction> transaktionen, bool isMonthGroup = false) : List<Transaction>(transaktionen)
{
    public DateTime Datum { get; } = datum;
    public string DatumFormatiert { get; } = isMonthGroup
        ? datum.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture)
        : datum.ToString("dd. MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
}
