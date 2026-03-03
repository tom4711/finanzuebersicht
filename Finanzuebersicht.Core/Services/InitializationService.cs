using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class InitializationService
{
    private readonly ICategoryRepository _categoryRepository;

    public InitializationService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task InitializeAsync()
    {
        var kategorien = await _categoryRepository.GetCategoriesAsync();
        if (kategorien.Count > 0) return;

        var standardKategorien = new List<Category>
        {
            new() { Name = "Lebensmittel", Icon = "🛒", Color = "#34C759", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Lebensmittel" },
            new() { Name = "Transport", Icon = "🚗", Color = "#007AFF", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Transport" },
            new() { Name = "Wohnen", Icon = "🏠", Color = "#FF9500", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Wohnen" },
            new() { Name = "Unterhaltung", Icon = "🎬", Color = "#AF52DE", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Unterhaltung" },
            new() { Name = "Gesundheit", Icon = "💊", Color = "#FF2D55", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Gesundheit" },
            new() { Name = "Gehalt", Icon = "💼", Color = "#34C759", Typ = TransactionType.Einnahme, SystemKey = "SysCat_Gehalt" },
            new() { Name = "Sonstiges", Icon = "📦", Color = "#A2845E", Typ = TransactionType.Ausgabe, SystemKey = "SysCat_Sonstiges" },
        };

        foreach (var kategorie in standardKategorien)
        {
            await _categoryRepository.SaveCategoryAsync(kategorie);
        }
    }
}
