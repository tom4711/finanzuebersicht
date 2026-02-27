namespace Finanzuebersicht.Models;

public class TransactionGroup : List<Transaction>
{
    public DateTime Datum { get; }
    public string DatumFormatiert { get; }

    public TransactionGroup(DateTime datum, IEnumerable<Transaction> transaktionen)
        : base(transaktionen)
    {
        Datum = datum;
        DatumFormatiert = datum.ToString("dd. MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
