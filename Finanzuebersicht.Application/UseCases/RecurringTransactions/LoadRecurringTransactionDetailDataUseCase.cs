using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class LoadRecurringTransactionDetailDataUseCase(ICategoryRepository categoryRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<RecurringTransactionDetailData> ExecuteAsync(string? selectedCategoryId)
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        var selectedCategory = selectedCategoryId == null
            ? null
            : categories.FirstOrDefault(c => c.Id == selectedCategoryId);

        return new RecurringTransactionDetailData
        {
            Kategorien = categories,
            SelectedKategorie = selectedCategory
        };
    }
}

public class RecurringTransactionDetailData
{
    public List<Category> Kategorien { get; set; } = [];
    public Category? SelectedKategorie { get; set; }
}