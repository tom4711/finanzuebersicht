using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly ILogger<AboutViewModel>? _logger;

    public string AppVersion { get; }
    public string BuildInfo { get; }
    public List<LibraryInfo> Libraries { get; } = [];

    public AboutViewModel(ILogger<AboutViewModel>? logger = null)
    {
        _logger = logger;

        var assembly = Assembly.GetExecutingAssembly();
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unbekannt";

        AppVersion = infoVersion.Contains('+')
            ? infoVersion[..infoVersion.IndexOf('+')]
            : infoVersion;
        BuildInfo = infoVersion.Contains('+')
            ? infoVersion[(infoVersion.IndexOf('+') + 1)..]
            : string.Empty;

        PopulateLibraries();
    }

    private void PopulateLibraries()
    {
        try
        {
            var entry = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
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

            var knownAssemblies = new[] { "CommunityToolkit.Mvvm", "CommunityToolkit.Maui", "Nerdbank.GitVersioning" };
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
