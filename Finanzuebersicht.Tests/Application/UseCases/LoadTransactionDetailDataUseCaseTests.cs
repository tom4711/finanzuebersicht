using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadTransactionDetailDataUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_SelectsCategory_WhenIdExists()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" },
            new() { Id = "cat-2", Name = "Miete" }
        });

        var sut = new LoadTransactionDetailDataUseCase(categoryRepository);

        var result = await sut.ExecuteAsync("cat-2");

        Assert.Equal(2, result.Kategorien.Count);
        Assert.NotNull(result.SelectedKategorie);
        Assert.Equal("cat-2", result.SelectedKategorie!.Id);
        await categoryRepository.Received(1).GetCategoriesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullSelection_WhenIdNotFound()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" }
        });

        var sut = new LoadTransactionDetailDataUseCase(categoryRepository);

        var result = await sut.ExecuteAsync("cat-x");

        Assert.Single(result.Kategorien);
        Assert.Null(result.SelectedKategorie);
        await categoryRepository.Received(1).GetCategoriesAsync();
    }
}