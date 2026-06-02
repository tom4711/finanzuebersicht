using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class SaveTransferUseCase(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task ExecuteAsync(
        string fromAccountId,
        string toAccountId,
        decimal amount,
        DateTime date,
        string? title = null,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(fromAccountId) || string.IsNullOrWhiteSpace(toAccountId))
            throw new InvalidOperationException("Source and target account are required.");
        if (fromAccountId == toAccountId)
            throw new InvalidOperationException("Source and target account must be different.");
        if (amount <= 0)
            throw new InvalidOperationException("Transfer amount must be greater than zero.");

        var accounts = await _accountRepository.GetAccountsAsync();
        var source = accounts.FirstOrDefault(a => a.Id == fromAccountId);
        var target = accounts.FirstOrDefault(a => a.Id == toAccountId);
        if (source == null || target == null)
            throw new InvalidOperationException("Source or target account not found.");
        if (source.IsArchived || target.IsArchived)
            throw new InvalidOperationException("Archived accounts cannot be used for new transfers.");

        var transferGroupId = Guid.NewGuid().ToString();
        var transferTitle = string.IsNullOrWhiteSpace(title) ? "Umbuchung" : title.Trim();
        var transferNote = note ?? string.Empty;

        var outgoing = new Transaction
        {
            Betrag = amount,
            Titel = transferTitle,
            Verwendungszweck = transferNote,
            Datum = date,
            Typ = TransactionType.Ausgabe,
            KategorieId = string.Empty,
            AccountId = fromAccountId,
            IsTransfer = true,
            TransferGroupId = transferGroupId
        };

        var incoming = new Transaction
        {
            Betrag = amount,
            Titel = transferTitle,
            Verwendungszweck = transferNote,
            Datum = date,
            Typ = TransactionType.Einnahme,
            KategorieId = string.Empty,
            AccountId = toAccountId,
            IsTransfer = true,
            TransferGroupId = transferGroupId
        };

        await _transactionRepository.SaveTransactionsAsync([outgoing, incoming]);
    }
}
