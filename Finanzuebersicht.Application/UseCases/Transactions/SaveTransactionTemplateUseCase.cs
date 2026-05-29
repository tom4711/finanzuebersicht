using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class SaveTransactionTemplateUseCase(ITransactionTemplateRepository repository)
{
    private readonly ITransactionTemplateRepository _repository = repository;

    public Task ExecuteAsync(TransactionTemplate template, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _repository.SaveTransactionTemplateAsync(template);
    }
}
