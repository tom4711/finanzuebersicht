using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;

    public DeleteCategoryUseCase(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringTransactionRepository)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task ExecuteAsync(string categoryId)
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        if (!categories.Any(c => c.Id == categoryId))
            return;

        var fallbackCategory = categories
            .FirstOrDefault(c => c.SystemKey == "SysCat_Sonstiges" && c.Id != categoryId)
            ?? categories.FirstOrDefault(c => c.Id != categoryId);

        if (fallbackCategory == null)
        {
            fallbackCategory = new Category
            {
                Name = "Sonstiges",
                Icon = "📦",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = "SysCat_Sonstiges"
            };

            await _categoryRepository.SaveCategoryAsync(fallbackCategory);
        }

        var transactions = await _transactionRepository.GetTransactionsAsync(DateTime.MinValue, DateTime.MaxValue);
        foreach (var transaction in transactions.Where(t => t.KategorieId == categoryId))
        {
            transaction.KategorieId = fallbackCategory.Id;
            await _transactionRepository.SaveTransactionAsync(transaction);
        }

        var recurringTransactions = await _recurringTransactionRepository.GetRecurringTransactionsAsync();
        foreach (var recurring in recurringTransactions.Where(r => r.KategorieId == categoryId))
        {
            recurring.KategorieId = fallbackCategory.Id;
            await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
        }

        await _categoryRepository.DeleteCategoryAsync(categoryId);
    }
}