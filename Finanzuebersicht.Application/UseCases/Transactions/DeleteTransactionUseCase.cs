using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class DeleteTransactionUseCase(ITransactionRepository transactionRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task ExecuteAsync(string transactionId)
    {
        await _transactionRepository.DeleteTransactionAsync(transactionId);
    }
}