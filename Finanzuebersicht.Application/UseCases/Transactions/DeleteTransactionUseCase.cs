using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class DeleteTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;

    public DeleteTransactionUseCase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task ExecuteAsync(string transactionId)
    {
        await _transactionRepository.DeleteTransactionAsync(transactionId);
    }
}