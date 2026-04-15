namespace Finanzuebersicht.Models;

public class SparZiel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Titel { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎯";
    public decimal ZielBetrag { get; set; }
    public decimal AktuellerBetrag { get; set; }
    public DateTime? Faelligkeitsdatum { get; set; }
}
