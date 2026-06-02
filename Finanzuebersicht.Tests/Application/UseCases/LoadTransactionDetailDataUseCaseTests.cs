using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadTransactionDetailDataUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_SelectsCategory_WhenIdExists()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" },
            new() { Id = "cat-2", Name = "Miete" }
        });
        accountRepository.GetAccountsAsync().Returns(new List<Account>
        {
            new() { Id = "acc-1", Name = "Girokonto", Type = AccountType.Girokonto }
        });

        var sut = new LoadTransactionDetailDataUseCase(categoryRepository, accountRepository);

        var result = await sut.ExecuteAsync("cat-2");

        Assert.Equal(2, result.Kategorien.Count);
        Assert.Single(result.Accounts);
        Assert.NotNull(result.SelectedKategorie);
        Assert.Equal("cat-2", result.SelectedKategorie!.Id);
        Assert.NotNull(result.SelectedAccount);
        Assert.Equal("acc-1", result.SelectedAccount!.Id);
        await categoryRepository.Received(1).GetCategoriesAsync();
        await accountRepository.Received(1).GetAccountsAsync();
    }

    [Fact]
    public async Task ExecuteAsync_SelectsFallback_WhenIdNotFound()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" },
            new() { Id = "cat-2", Name = "Sonstiges", SystemKey = Finanzuebersicht.Constants.SystemCategoryKeys.Sonstiges }
        });
        accountRepository.GetAccountsAsync().Returns(new List<Account>
        {
            new() { Id = "acc-1", Name = "Girokonto", Type = AccountType.Girokonto, SystemKey = Finanzuebersicht.Constants.SystemAccountKeys.Default }
        });

        var sut = new LoadTransactionDetailDataUseCase(categoryRepository, accountRepository);

        var result = await sut.ExecuteAsync("cat-x");

        Assert.Equal(2, result.Kategorien.Count);
        Assert.Single(result.Accounts);
        Assert.NotNull(result.SelectedKategorie);
        Assert.Equal("cat-2", result.SelectedKategorie!.Id);
        Assert.NotNull(result.SelectedAccount);
        Assert.Equal("acc-1", result.SelectedAccount!.Id);
        await categoryRepository.Received(1).GetCategoriesAsync();
        await accountRepository.Received(1).GetAccountsAsync();
    }

    [Fact]
    public async Task ExecuteAsync_SelectsFirstCategory_WhenFallbackMissing()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" }
        });
        accountRepository.GetAccountsAsync().Returns(new List<Account>());

        var sut = new LoadTransactionDetailDataUseCase(categoryRepository, accountRepository);

        var result = await sut.ExecuteAsync("cat-x");

        Assert.Single(result.Kategorien);
        Assert.Empty(result.Accounts);
        Assert.NotNull(result.SelectedKategorie);
        Assert.Equal("cat-1", result.SelectedKategorie!.Id);
    }
}