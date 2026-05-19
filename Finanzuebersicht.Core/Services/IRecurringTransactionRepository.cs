using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

public interface IRecurringTransactionRepository
{
    Task<List<RecurringTransaction>> GetRecurringTransactionsAsync();
    Task SaveRecurringTransactionAsync(RecurringTransaction recurring);
    Task DeleteRecurringTransactionAsync(string id);
    Task ReplaceAllRecurringTransactionsAsync(IEnumerable<RecurringTransaction> recurring);
}
