using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class LoadCategoriesUseCase(ICategoryRepository categoryRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<List<Category>> ExecuteAsync()
    {
        return await _categoryRepository.GetCategoriesAsync();
    }
}