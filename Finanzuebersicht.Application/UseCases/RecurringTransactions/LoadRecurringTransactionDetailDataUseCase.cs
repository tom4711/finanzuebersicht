using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class LoadRecurringTransactionDetailDataUseCase(
    ICategoryRepository categoryRepository,
    IAccountRepository accountRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<RecurringTransactionDetailData> ExecuteAsync(
        string? selectedCategoryId,
        string? selectedAccountId = null,
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        var accounts = await _accountRepository.GetAccountsAsync();
        var selectedCategory = selectedCategoryId == null
            ? null
            : categories.FirstOrDefault(c => c.Id == selectedCategoryId);
        var selectedAccount = selectedAccountId == null
            ? null
            : accounts.FirstOrDefault(a => a.Id == selectedAccountId);

        selectedAccount ??= accounts.FirstOrDefault(a => a.SystemKey == Finanzuebersicht.Constants.SystemAccountKeys.Default)
            ?? accounts.FirstOrDefault(a => !a.IsArchived)
            ?? accounts.FirstOrDefault();

        var visibleAccounts = accounts
            .Where(a => !a.IsArchived)
            .ToList();

        if (selectedAccount?.IsArchived == true && visibleAccounts.All(a => a.Id != selectedAccount.Id))
            visibleAccounts.Add(selectedAccount);

        return new RecurringTransactionDetailData
        {
            Kategorien = categories,
            Accounts = visibleAccounts.OrderBy(a => a.Name).ToList(),
            SelectedKategorie = selectedCategory,
            SelectedAccount = selectedAccount
        };
    }
}

public class RecurringTransactionDetailData
{
    public List<Category> Kategorien { get; set; } = [];
    public List<Account> Accounts { get; set; } = [];
    public Category? SelectedKategorie { get; set; }
    public Account? SelectedAccount { get; set; }
}