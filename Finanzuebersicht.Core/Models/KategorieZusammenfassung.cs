namespace Finanzuebersicht.Models;

public class KategorieZusammenfassung
{
    public Category Kategorie { get; set; } = new();
    public decimal Summe { get; set; }
    public double Prozent { get; set; }
}
