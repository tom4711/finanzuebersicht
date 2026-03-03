using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum);
    Task SaveTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(string id);
}
