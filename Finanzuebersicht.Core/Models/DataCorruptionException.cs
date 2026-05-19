namespace Finanzuebersicht.Models;

/// <summary>
/// Thrown when a data file exists but cannot be parsed due to corruption or invalid content.
/// Distinct from "file not found" — signals that data may have been lost or damaged.
/// </summary>
public class DataCorruptionException : Exception
{
    public string FilePath { get; }

    public DataCorruptionException(string filePath, Exception innerException)
        : base($"Data file is corrupted and cannot be loaded: {filePath}", innerException)
    {
        FilePath = filePath;
    }
}
