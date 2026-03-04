using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class SaveCategoryDetailUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public SaveCategoryDetailUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task ExecuteAsync(
        Category? existingCategory,
        string name,
        string icon,
        string color,
        TransactionType typ)
    {
        var category = existingCategory ?? new Category();
        category.Name = name;
        category.Icon = icon;
        category.Color = color;
        category.Typ = typ;

        await _categoryRepository.SaveCategoryAsync(category);
    }
}