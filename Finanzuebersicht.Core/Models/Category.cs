namespace Finanzuebersicht.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "💰";
    public string Color { get; set; } = "#007AFF";
    public TransactionType Typ { get; set; } = TransactionType.Ausgabe;

    /// <summary>
    /// Optionaler Schlüssel für vom System angelegte Kategorien (z.B. Finanzuebersicht.Core.Constants.SystemCategoryKeys.Lebensmittel).
    /// Wird zur Laufzeit in die übersetzte Bezeichnung aufgelöst.
    /// Null bei nutzerdefinierten Kategorien – diese verwenden immer Name direkt.
    /// </summary>
    public string? SystemKey { get; set; }
}
