using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class SaveTransactionDetailUseCase
{
    private readonly ITransactionRepository _transactionRepository;

    public SaveTransactionDetailUseCase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task ExecuteAsync(
        Transaction? existingTransaction,
        decimal betrag,
        string titel,
        DateTime datum,
        string kategorieId,
        TransactionType typ)
    {
        var transaction = existingTransaction ?? new Transaction();
        transaction.Betrag = betrag;
        transaction.Titel = titel;
        transaction.Datum = datum;
        transaction.KategorieId = kategorieId;
        transaction.Typ = typ;

        await _transactionRepository.SaveTransactionAsync(transaction);
    }
}