using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task ExecuteAsync(string categoryId)
    {
        await _categoryRepository.DeleteCategoryAsync(categoryId);
    }
}