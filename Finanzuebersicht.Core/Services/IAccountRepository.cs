using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

public interface IAccountRepository
{
    Task<List<Account>> GetAccountsAsync();
    Task SaveAccountAsync(Account account);
    Task DeleteAccountAsync(string id);
    Task ReplaceAllAccountsAsync(IEnumerable<Account> accounts);
}
