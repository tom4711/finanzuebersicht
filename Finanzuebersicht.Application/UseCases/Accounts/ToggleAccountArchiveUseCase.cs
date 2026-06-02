using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Accounts;

public class ToggleAccountArchiveUseCase(IAccountRepository accountRepository)
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<Account> ExecuteAsync(Account account, bool isArchived, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (account.IsSystemAccount)
            throw new InvalidOperationException("System account cannot be archived.");

        account.IsArchived = isArchived;
        await _accountRepository.SaveAccountAsync(account);
        return account;
    }
}
