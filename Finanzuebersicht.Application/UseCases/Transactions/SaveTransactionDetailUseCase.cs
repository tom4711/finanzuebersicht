using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class SaveTransactionDetailUseCase(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

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
        if (existingTransaction?.IsTransfer == true)
            throw new InvalidOperationException("Transfers must be edited through the transfer flow.");

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            var accounts = await _accountRepository.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new InvalidOperationException("Selected account not found.");
            if (account.IsArchived && (existingTransaction == null || existingTransaction.AccountId != accountId))
                throw new InvalidOperationException("Archived account cannot be assigned to new transactions.");
        }

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