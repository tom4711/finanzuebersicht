using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class SaveCategoryDetailUseCase(ICategoryRepository categoryRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<Category> ExecuteAsync(
        Category? existingCategory,
        string name,
        string icon,
        string color,
        TransactionType typ, CancellationToken cancellationToken = default)
    {
        var category = existingCategory ?? new Category();
        category.Name = name;
        category.Icon = icon;
        category.Color = color;
        category.Typ = typ;

        await _categoryRepository.SaveCategoryAsync(category);
        return category;
    }
}