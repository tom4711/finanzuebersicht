
namespace Finanzuebersicht.Application.UseCases.Transactions;

public class DeleteTransactionUseCase(ITransactionRepository transactionRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task ExecuteAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        await _transactionRepository.DeleteTransactionAsync(transactionId);
    }
}