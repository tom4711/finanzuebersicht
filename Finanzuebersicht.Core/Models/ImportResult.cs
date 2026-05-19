namespace Finanzuebersicht.Models;

/// <summary>
/// Result of a CSV import operation with a full breakdown of what happened.
/// </summary>
public class ImportResult
{
    /// <summary>Transactions that were successfully parsed, validated, and saved.</summary>
    public IReadOnlyList<Transaction> Imported { get; init; } = [];

    /// <summary>Transactions that were skipped due to being duplicates of existing records.</summary>
    public IReadOnlyList<Transaction> Duplicates { get; init; } = [];

    /// <summary>Rows skipped because they were malformed (e.g., missing date or amount).</summary>
    public int SkippedMalformed { get; init; }

    /// <summary>Transactions that were valid and not duplicates but failed to save.</summary>
    public IReadOnlyList<string> SaveErrors { get; init; } = [];

    /// <summary>Top-level error message if the import failed entirely (e.g., no parser matched, stream read error).</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Whether the import completed without a top-level error.</summary>
    public bool Success => ErrorMessage is null;

    /// <summary>Total number of records processed (includes duplicates and malformed).</summary>
    public int TotalProcessed => Imported.Count + Duplicates.Count + SkippedMalformed + SaveErrors.Count;
}
