using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadCategoriesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsCategoriesFromRepository()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var expected = new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel", Icon = "🛒", Color = "#00FF00" },
            new() { Id = "cat-2", Name = "Miete", Icon = "🏠", Color = "#0000FF" }
        };
        categoryRepository.GetCategoriesAsync().Returns(expected);

        var sut = new LoadCategoriesUseCase(categoryRepository);

        var result = await sut.ExecuteAsync();

        Assert.Equal(expected, result);
        await categoryRepository.Received(1).GetCategoriesAsync();
    }
}