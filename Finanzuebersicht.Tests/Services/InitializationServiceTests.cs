using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class InitializationServiceTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();

    [Fact]
    public async Task InitializeAsync_ErstelltStandardKategorien_WennKeineVorhanden()
    {
        _categoryRepository.GetCategoriesAsync().Returns(new List<Category>());

        var service = new InitializationService(_categoryRepository);
        await service.InitializeAsync();

        // 7 Standardkategorien werden erstellt
        await _categoryRepository.Received(7).SaveCategoryAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task InitializeAsync_ErstelltNichts_WennKategorienVorhanden()
    {
        _categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "1", Name = "Existiert", Icon = "📦", Color = "#007AFF", Typ = TransactionType.Ausgabe }
        });

        var service = new InitializationService(_categoryRepository);
        await service.InitializeAsync();

        await _categoryRepository.DidNotReceive().SaveCategoryAsync(Arg.Any<Category>());
    }
}
