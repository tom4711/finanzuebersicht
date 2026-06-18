using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class RestoreTransactionUseCase(ITransactionRepository transactionRepository)
{
    public async Task ExecuteAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await transactionRepository.SaveTransactionAsync(transaction);
    }
}
