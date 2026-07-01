using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly Assembly _appAssembly;
    private readonly ILogger<AboutViewModel>? _logger;

    public string AppVersion { get; }
    public string BuildInfo { get; }
    public List<LibraryInfo> Libraries { get; } = [];

    public AboutViewModel(Assembly? appAssembly = null, ILogger<AboutViewModel>? logger = null)
    {
        _appAssembly = appAssembly ?? Assembly.GetExecutingAssembly();
        _logger = logger;

        var infoVersion = _appAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unbekannt";
        (AppVersion, BuildInfo) = ParseVersionDisplay(infoVersion);

        PopulateLibraries();
    }

    /// <summary>
    /// Maps Nerdbank informational version (e.g. <c>1.17.9+ea2311c9de</c>) to user-facing
    /// release line (<c>1.17</c>) and build metadata (<c>9 · ea2311c9de</c>).
    /// </summary>
    public static (string AppVersion, string BuildInfo) ParseVersionDisplay(string infoVersion)
    {
        var core = infoVersion;
        string? metadata = null;

        var plusIdx = infoVersion.IndexOf('+');
        var minusIdx = infoVersion.IndexOf('-');
        if (plusIdx >= 0)
        {
            core = infoVersion[..plusIdx];
            metadata = infoVersion[(plusIdx + 1)..];
        }
        else if (minusIdx >= 0)
        {
            core = infoVersion[..minusIdx];
            metadata = infoVersion[(minusIdx + 1)..];
        }

        if (Version.TryParse(core, out var version))
        {
            var appVersion = $"{version.Major}.{version.Minor}";
            var buildParts = new List<string>();
            if (version.Build > 0)
                buildParts.Add(version.Build.ToString());
            if (!string.IsNullOrWhiteSpace(metadata))
                buildParts.Add(metadata);
            return (appVersion, string.Join(" · ", buildParts));
        }

        return (core, metadata ?? string.Empty);
    }

    private void PopulateLibraries()
    {
        try
        {
            var entry = Assembly.GetEntryAssembly() ?? _appAssembly;
            var refs = entry.GetReferencedAssemblies();

            foreach (var reference in refs.OrderBy(r => r.Name))
            {
                var referenceName = reference.Name;
                if (string.IsNullOrWhiteSpace(referenceName) ||
                    referenceName.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                    referenceName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                    referenceName.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                    referenceName.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Libraries.Add(new LibraryInfo(referenceName, $"Version {reference.Version}"));
            }

            var knownAssemblies = new[] { "CommunityToolkit.Mvvm", "CommunityToolkit.Maui" };
            foreach (var knownAssembly in knownAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(knownAssembly));
                    var assemblyName = assembly.GetName().Name;
                    if (string.IsNullOrWhiteSpace(assemblyName) || Libraries.Any(l => l.Name == assemblyName))
                    {
                        continue;
                    }

                    Libraries.Add(new LibraryInfo(assemblyName, $"Version {assembly.GetName().Version}"));
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Konnte bekannte Assembly '{Name}' nicht laden", knownAssembly);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Fehler beim Ermitteln der verwendeten Bibliotheken");
        }
    }
}

public record LibraryInfo(string Name, string Description);
