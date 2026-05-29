using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

public interface ITransactionTemplateRepository
{
    Task<List<TransactionTemplate>> GetTransactionTemplatesAsync();
    Task SaveTransactionTemplateAsync(TransactionTemplate template);
    Task DeleteTransactionTemplateAsync(string id);
    Task ReplaceAllTransactionTemplatesAsync(IEnumerable<TransactionTemplate> templates);
}
