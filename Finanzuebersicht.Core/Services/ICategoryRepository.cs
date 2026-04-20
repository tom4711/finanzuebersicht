using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync();
    Task SaveCategoryAsync(Category category);
    Task DeleteCategoryAsync(string id);
    Task ReplaceAllCategoriesAsync(IEnumerable<Category> categories);
}
