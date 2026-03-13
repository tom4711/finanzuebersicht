using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

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
        TransactionType typ,
        string verwendungszweck)
    {
        var transaction = existingTransaction ?? new Transaction();
        transaction.Betrag = betrag;
        transaction.Titel = titel;
        transaction.Datum = datum;
        transaction.KategorieId = kategorieId;
        transaction.Typ = typ;
        transaction.Verwendungszweck = verwendungszweck ?? string.Empty;

        await _transactionRepository.SaveTransactionAsync(transaction);
    }
}