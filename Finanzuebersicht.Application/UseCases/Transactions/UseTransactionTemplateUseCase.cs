using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class UseTransactionTemplateUseCase(
    ITransactionTemplateRepository repository,
    IClock clock)
{
    private readonly ITransactionTemplateRepository _repository = repository;
    private readonly IClock _clock = clock;

    public async Task ExecuteAsync(TransactionTemplate template, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        template.LastUsedAt = _clock.UtcNow;
        template.UseCount++;
        await _repository.SaveTransactionTemplateAsync(template);
    }
}
