using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class LoadCategoriesUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public LoadCategoriesUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> ExecuteAsync()
    {
        return await _categoryRepository.GetCategoriesAsync();
    }
}