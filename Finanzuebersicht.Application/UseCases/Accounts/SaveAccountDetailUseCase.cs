using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Accounts;

public class SaveAccountDetailUseCase(IAccountRepository accountRepository)
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<Account> ExecuteAsync(
        Account? existingAccount,
        string name,
        AccountType type,
        bool isArchived = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var account = existingAccount ?? new Account();
        account.Name = name;
        account.Type = type;
        account.IsArchived = account.IsSystemAccount ? false : isArchived;

        await _accountRepository.SaveAccountAsync(account);
        return account;
    }
}
