namespace Finanzuebersicht.Models;

public class ImportPreviewResult
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public IReadOnlyList<ImportPreviewRow> Rows { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public bool Success => ErrorMessage is null;
    public int ReadyCount => Rows.Count(r => r.Status == ImportPreviewRowStatus.Ready);
    public int DuplicateCount => Rows.Count(r => r.Status == ImportPreviewRowStatus.Duplicate);
    public int InvalidCount => Rows.Count(r => r.Status == ImportPreviewRowStatus.Invalid);
    public int UncategorizedCount => Rows.Count(r => r.Status == ImportPreviewRowStatus.Uncategorized);
    public int IncludedCount => Rows.Count(r => r.IsIncluded);
}
