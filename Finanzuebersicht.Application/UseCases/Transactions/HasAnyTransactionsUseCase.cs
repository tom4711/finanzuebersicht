namespace Finanzuebersicht.Application.UseCases.Transactions;

public class HasAnyTransactionsUseCase(ITransactionRepository transactionRepository)
{
    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var transactions = await transactionRepository.GetTransactionsAsync(
            new DateTime(1900, 1, 1),
            new DateTime(2100, 12, 31, 23, 59, 59));
        return transactions.Count > 0;
    }
}
