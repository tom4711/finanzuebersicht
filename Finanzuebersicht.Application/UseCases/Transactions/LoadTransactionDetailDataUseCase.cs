using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class LoadTransactionDetailDataUseCase(ICategoryRepository categoryRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<TransactionDetailData> ExecuteAsync(string? selectedCategoryId)
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        var selectedCategory = selectedCategoryId == null
            ? null
            : categories.FirstOrDefault(c => c.Id == selectedCategoryId);

        selectedCategory ??= categories.FirstOrDefault(c => c.SystemKey == Finanzuebersicht.Constants.SystemCategoryKeys.Sonstiges)
            ?? categories.FirstOrDefault();

        return new TransactionDetailData
        {
            Kategorien = categories,
            SelectedKategorie = selectedCategory
        };
    }
}

public class TransactionDetailData
{
    public List<Category> Kategorien { get; set; } = [];
    public Category? SelectedKategorie { get; set; }
}