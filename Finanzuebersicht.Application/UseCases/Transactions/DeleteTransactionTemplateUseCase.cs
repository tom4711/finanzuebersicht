namespace Finanzuebersicht.Application.UseCases.Transactions;

public class DeleteTransactionTemplateUseCase(ITransactionTemplateRepository repository)
{
    private readonly ITransactionTemplateRepository _repository = repository;

    public Task ExecuteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _repository.DeleteTransactionTemplateAsync(id);
    }
}
