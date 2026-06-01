using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class LoadTransactionTemplatesUseCase(ITransactionTemplateRepository repository)
{
    private readonly ITransactionTemplateRepository _repository = repository;

    public async Task<List<TransactionTemplate>> ExecuteAsync(int limit = 6, CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetTransactionTemplatesAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return [.. templates.Take(limit)];
    }
}
