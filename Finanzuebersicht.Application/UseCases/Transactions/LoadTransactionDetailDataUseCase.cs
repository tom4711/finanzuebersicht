using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Transactions;

public class LoadTransactionDetailDataUseCase(
    ICategoryRepository categoryRepository,
    IAccountRepository accountRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<TransactionDetailData> ExecuteAsync(
        string? selectedCategoryId,
        string? selectedAccountId = null,
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        var accounts = await _accountRepository.GetAccountsAsync();
        var selectedCategory = selectedCategoryId == null
            ? null
            : categories.FirstOrDefault(c => c.Id == selectedCategoryId);

        selectedCategory ??= categories.FirstOrDefault(c => c.SystemKey == Finanzuebersicht.Constants.SystemCategoryKeys.Sonstiges)
            ?? categories.FirstOrDefault();

        var selectedAccount = selectedAccountId == null
            ? null
            : accounts.FirstOrDefault(a => a.Id == selectedAccountId);

        selectedAccount ??= accounts.FirstOrDefault(a => a.SystemKey == Finanzuebersicht.Constants.SystemAccountKeys.Default)
            ?? accounts.FirstOrDefault();

        return new TransactionDetailData
        {
            Kategorien = categories,
            Accounts = accounts,
            SelectedKategorie = selectedCategory,
            SelectedAccount = selectedAccount
        };
    }
}

public class TransactionDetailData
{
    public List<Category> Kategorien { get; set; } = [];
    public List<Account> Accounts { get; set; } = [];
    public Category? SelectedKategorie { get; set; }
    public Account? SelectedAccount { get; set; }
}