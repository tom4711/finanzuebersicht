namespace Finanzuebersicht.Models;

public class ImportPreviewRow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int SourceIndex { get; set; }
    public bool IsIncluded { get; set; }
    public ImportPreviewRowStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    public Transaction Transaction { get; set; } = new();
}
