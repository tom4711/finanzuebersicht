using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface IBudgetRepository
{
    Task<List<CategoryBudget>> GetBudgetsAsync();
    Task SaveBudgetAsync(CategoryBudget budget);
    Task DeleteBudgetAsync(string id);
    Task<CategoryBudget?> GetBudgetForCategoryAsync(string kategorieId, int year, int month);
    Task ReplaceAllBudgetsAsync(IEnumerable<CategoryBudget> budgets);
}
