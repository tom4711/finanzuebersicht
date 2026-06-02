using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class SaveTransactionDetailUseCase(ITransactionRepository transactionRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task ExecuteAsync(
        Transaction? existingTransaction,
        decimal betrag,
        string titel,
        DateTime datum,
        string kategorieId,
        string? accountId,
        TransactionType typ,
        string verwendungszweck, CancellationToken cancellationToken = default)
    {
        var transaction = existingTransaction ?? new Transaction();
        transaction.Betrag = betrag;
        transaction.Titel = titel;
        transaction.Datum = datum;
        transaction.KategorieId = kategorieId;
        transaction.AccountId = accountId ?? transaction.AccountId ?? string.Empty;
        transaction.Typ = typ;
        transaction.Verwendungszweck = verwendungszweck ?? string.Empty;

        await _transactionRepository.SaveTransactionAsync(transaction);
    }
}