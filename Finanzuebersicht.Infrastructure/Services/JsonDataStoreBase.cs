using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Services;

/// <summary>
/// Base class for JSON-based data stores. Provides shared JSON serialization,
/// deserialization, and thread-safe file I/O with configurable data directory.
/// </summary>
public abstract class JsonDataStoreBase : IDisposable
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected readonly string DataDir;
    protected readonly ILogger? Logger;
    protected readonly SemaphoreSlim StoreLock = new(1, 1);

    protected JsonDataStoreBase(string dataDir, ILogger? logger = null)
    {
        DataDir = dataDir;
        Logger = logger;
        Directory.CreateDirectory(DataDir);
    }

    /// <summary>
    /// Loads a list of items from a JSON file. Returns empty list if file doesn't exist
    /// or JSON is corrupted (with logged warning).
    /// </summary>
    protected async Task<List<T>> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            return [];

        var json = await File.ReadAllTextAsync(path);
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            Logger?.LogWarning(ex, "Error deserializing {Path}", path);
            return [];
        }
    }

    /// <summary>
    /// Saves a list of items to a JSON file.
    /// </summary>
    protected static async Task SaveAsync<T>(string path, List<T> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    /// <summary>
    /// Replaces the entire collection in the JSON file with the given items.
    /// Thread-safe via <see cref="StoreLock"/>. One file write regardless of collection size —
    /// use for bulk restore operations instead of individual Save/Delete calls.
    /// </summary>
    protected async Task ReplaceAllAsync<T>(string path, IEnumerable<T> items)
    {
        await StoreLock.WaitAsync();
        try
        {
            await SaveAsync(path, [..items]);
        }
        finally
        {
            StoreLock.Release();
        }
    }

    public virtual void Dispose()
    {
        StoreLock.Dispose();
    }
}
