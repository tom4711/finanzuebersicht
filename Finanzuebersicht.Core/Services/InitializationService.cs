using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class InitializationService
{
    private readonly IDataService _dataService;

    public InitializationService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task InitializeAsync()
    {
        var kategorien = await _dataService.GetCategoriesAsync();
        if (kategorien.Count > 0) return;

        var standardKategorien = new List<Category>
        {
            new() { Name = "Lebensmittel", Icon = "🛒", Color = "#34C759", Typ = TransactionType.Ausgabe },
            new() { Name = "Transport", Icon = "🚗", Color = "#007AFF", Typ = TransactionType.Ausgabe },
            new() { Name = "Wohnen", Icon = "🏠", Color = "#FF9500", Typ = TransactionType.Ausgabe },
            new() { Name = "Unterhaltung", Icon = "🎬", Color = "#AF52DE", Typ = TransactionType.Ausgabe },
            new() { Name = "Gesundheit", Icon = "💊", Color = "#FF2D55", Typ = TransactionType.Ausgabe },
            new() { Name = "Gehalt", Icon = "💼", Color = "#34C759", Typ = TransactionType.Einnahme },
            new() { Name = "Sonstiges", Icon = "📦", Color = "#A2845E", Typ = TransactionType.Ausgabe },
        };

        foreach (var kategorie in standardKategorien)
        {
            await _dataService.SaveCategoryAsync(kategorie);
        }
    }
}
