using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class DeleteCategoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var sut = new DeleteCategoryUseCase(categoryRepository);

        await sut.ExecuteAsync("cat-1");

        await categoryRepository.Received(1).DeleteCategoryAsync("cat-1");
    }
}