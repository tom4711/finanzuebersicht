using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Tests.Application.UseCases.Accounts;

public class AccountUseCaseTests
{
    [Fact]
    public async Task SaveAccountDetailUseCase_PersistsNameAndType()
    {
        var repository = Substitute.For<IAccountRepository>();
        repository.SaveAccountAsync(Arg.Any<Account>()).Returns(Task.CompletedTask);

        var sut = new SaveAccountDetailUseCase(repository);

        var saved = await sut.ExecuteAsync(null, "Tagesgeld", AccountType.Tagesgeld);

        await repository.Received(1).SaveAccountAsync(Arg.Is<Account>(a =>
            a.Name == "Tagesgeld" &&
            a.Type == AccountType.Tagesgeld));
        Assert.Equal("Tagesgeld", saved.Name);
        Assert.Equal(AccountType.Tagesgeld, saved.Type);
    }

    [Fact]
    public async Task DeleteAccountUseCase_ReassignsTransactionsAndTemplates()
    {
        var defaultAccount = new Account { Id = "acc-default", Name = "Girokonto", SystemKey = Finanzuebersicht.Constants.SystemAccountKeys.Default };
        var accountToDelete = new Account { Id = "acc-old", Name = "Alt" };

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.GetAccountsAsync().Returns(new List<Account> { defaultAccount, accountToDelete });
        accountRepository.DeleteAccountAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { Id = "t1", AccountId = "acc-old" }
            });
        transactionRepository.SaveTransactionAsync(Arg.Any<Transaction>()).Returns(Task.CompletedTask);

        var templateRepository = Substitute.For<ITransactionTemplateRepository>();
        templateRepository.GetTransactionTemplatesAsync().Returns(new List<TransactionTemplate>
        {
            new() { Id = "tpl1", AccountId = "acc-old" }
        });
        templateRepository.SaveTransactionTemplateAsync(Arg.Any<TransactionTemplate>()).Returns(Task.CompletedTask);

        var sut = new DeleteAccountUseCase(accountRepository, transactionRepository, templateRepository);

        await sut.ExecuteAsync("acc-old");

        await transactionRepository.Received(1).SaveTransactionAsync(Arg.Is<Transaction>(t => t.AccountId == "acc-default"));
        await templateRepository.Received(1).SaveTransactionTemplateAsync(Arg.Is<TransactionTemplate>(t => t.AccountId == "acc-default"));
        await accountRepository.Received(1).DeleteAccountAsync("acc-old");
    }

    [Fact]
    public async Task DeleteAccountUseCase_RejectsDefaultAccount()
    {
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.GetAccountsAsync().Returns(new List<Account>
        {
            new() { Id = "acc-default", Name = "Girokonto", SystemKey = Finanzuebersicht.Constants.SystemAccountKeys.Default }
        });

        var sut = new DeleteAccountUseCase(accountRepository, Substitute.For<ITransactionRepository>(), Substitute.For<ITransactionTemplateRepository>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync("acc-default"));
    }
}
