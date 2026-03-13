using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

/// <summary>
/// Categorizes transactions using regex pattern matching on payee/reference fields.
/// Patterns are loaded from a JSON configuration file (Data/categorization-rules.json).
/// </summary>
public class KeywordCategorizationStrategy : ICategorizationStrategy
{
    private readonly ILogger<KeywordCategorizationStrategy>? _logger;
    private readonly Dictionary<string, List<Regex>> _rules = [];

    public int Priority => 10;  // Run early, before historical matching
    public string Name => "Keyword Pattern Matching";

    public KeywordCategorizationStrategy(ILogger<KeywordCategorizationStrategy>? logger = null)
    {
        _logger = logger;
        LoadRulesFromFile();
    }

    private void LoadRulesFromFile()
    {
        try
        {
            var rulesPath = FindRulesFile();
            if (rulesPath == null)
            {
                _logger?.LogWarning("Categorization rules file not found");
                return;
            }

            var rulesJson = File.ReadAllText(rulesPath);
            var parsedJsonDocument = JsonDocument.Parse(rulesJson);

            if (parsedJsonDocument.RootElement.TryGetProperty("rules", out var rulesElement) && rulesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var rule in rulesElement.EnumerateArray())
                {
                    if (rule.TryGetProperty("categoryName", out var categoryNameEl) &&
                        rule.TryGetProperty("patterns", out var patternsEl) &&
                        patternsEl.ValueKind == JsonValueKind.Array)
                    {
                        var categoryName = categoryNameEl.GetString();
                        if (categoryName == null) continue;

                        var patterns = new List<Regex>();
                        foreach (var patternEl in patternsEl.EnumerateArray())
                        {
                            if (patternEl.ValueKind == JsonValueKind.String)
                            {
                                var pattern = patternEl.GetString();
                                if (pattern != null)
                                {
                                    try
                                    {
                                        // Create regex pattern with word boundaries for whole-word matching
                                        patterns.Add(new Regex($@"\b{Regex.Escape(pattern)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger?.LogWarning($"Invalid regex pattern '{pattern}' for category '{categoryName}': {ex.Message}");
                                    }
                                }
                            }
                        }

                        if (patterns.Count > 0)
                        {
                            _rules[categoryName] = patterns;
                        }
                    }
                }
            }

            _logger?.LogInformation($"Loaded {_rules.Count} categorization rules");
            try { FileLogger.Append("KeywordCategorizationStrategy", $"Loaded {_rules.Count} rules"); } catch { }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading categorization rules");
            try { FileLogger.Append("KeywordCategorizationStrategy", $"Error loading rules: {ex.Message}"); } catch { }
        }
    }

    private string? FindRulesFile()
    {
        // Try multiple locations
        var candidates = new List<string>
        {
            // Path 1: In AppContext base directory (production)
            Path.Combine(AppContext.BaseDirectory, "Data", "categorization-rules.json"),

            // Path 2: Relative to current working directory (debugging/testing)
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "categorization-rules.json"),

            // Path 3: In parent directories (for tests)
            Path.Combine(Directory.GetCurrentDirectory(), "Services", "Data", "categorization-rules.json"),
            
            // Path 4: macOS/Mac Catalyst - Resources directory (Contents/Resources/Data)
            Path.Combine(AppContext.BaseDirectory, "..", "Resources", "Data", "categorization-rules.json"),
        };

        foreach (var path in candidates)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                _logger?.LogInformation("Found categorization rules at: {Path}", fullPath);
                return fullPath;
            }
        }

        _logger?.LogWarning("Categorization rules file not found in any of the expected locations");
        return null;
    }

    public Task<Category?> TryCategorizAsync(
        TransactionDto dto,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default)
    {
        if (_rules.Count == 0)
        {
            return Task.FromResult<Category?>(null);
        }

        var categoriesList = availableCategories.ToList();
        var searchText = GetSearchText(dto);

        if (string.IsNullOrEmpty(searchText))
        {
            return Task.FromResult<Category?>(null);
        }

        // Try to match any rule
        foreach (var rule in _rules)
        {
            var categoryName = rule.Key;
            var patterns = rule.Value;

            // Check if any pattern matches
            if (patterns.Any(p => p.IsMatch(searchText)))
            {
                // Find matching category
                var matchedCategory = categoriesList.FirstOrDefault(c => c.Name == categoryName);
                if (matchedCategory != null)
                {
                    return Task.FromResult<Category?>(matchedCategory);
                }
            }
        }

        return Task.FromResult<Category?>(null);
    }

    private static string GetSearchText(TransactionDto dto)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(dto.Zahlungsempfaenger))
            parts.Add(dto.Zahlungsempfaenger);

        if (!string.IsNullOrEmpty(dto.Zahlungspflichtige))
            parts.Add(dto.Zahlungspflichtige);

        if (!string.IsNullOrEmpty(dto.Verwendungszweck))
            parts.Add(dto.Verwendungszweck);

        return string.Join(" ", parts);
    }
}
