using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface IDataService
{
    // Categories
    Task<List<Category>> GetCategoriesAsync();
    Task SaveCategoryAsync(Category category);
    Task DeleteCategoryAsync(string id);

    // Transactions
    Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum);
    Task SaveTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(string id);

    // Recurring Transactions
    Task<List<RecurringTransaction>> GetRecurringTransactionsAsync();
    Task SaveRecurringTransactionAsync(RecurringTransaction recurring);
    Task DeleteRecurringTransactionAsync(string id);

    // Fällige wiederkehrende Zahlungen als Transactions erzeugen
    Task GeneratePendingRecurringTransactionsAsync();
}
