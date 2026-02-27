using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class InitializationServiceTests
{
    private readonly IDataService _dataService = Substitute.For<IDataService>();

    [Fact]
    public async Task InitializeAsync_ErstelltStandardKategorien_WennKeineVorhanden()
    {
        _dataService.GetCategoriesAsync().Returns(new List<Category>());

        var service = new InitializationService(_dataService);
        await service.InitializeAsync();

        // 7 Standardkategorien werden erstellt
        await _dataService.Received(7).SaveCategoryAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task InitializeAsync_ErstelltNichts_WennKategorienVorhanden()
    {
        _dataService.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "1", Name = "Existiert", Icon = "📦", Color = "#007AFF", Typ = TransactionType.Ausgabe }
        });

        var service = new InitializationService(_dataService);
        await service.InitializeAsync();

        await _dataService.DidNotReceive().SaveCategoryAsync(Arg.Any<Category>());
    }
}
