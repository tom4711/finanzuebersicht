using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadRecurringTransactionDetailDataUseCaseTests
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
            new() { Id = "acc-1", Name = "Girokonto", SystemKey = Finanzuebersicht.Constants.SystemAccountKeys.Default }
        });

        var sut = new LoadRecurringTransactionDetailDataUseCase(categoryRepository, accountRepository);

        var result = await sut.ExecuteAsync("cat-2", "acc-1");

        Assert.Equal(2, result.Kategorien.Count);
        Assert.NotNull(result.SelectedKategorie);
        Assert.Equal("cat-2", result.SelectedKategorie!.Id);
        Assert.NotNull(result.SelectedAccount);
        Assert.Equal("acc-1", result.SelectedAccount!.Id);
        await categoryRepository.Received(1).GetCategoriesAsync();
        await accountRepository.Received(1).GetAccountsAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullSelection_WhenIdNotFound()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        categoryRepository.GetCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = "cat-1", Name = "Lebensmittel" }
        });
        accountRepository.GetAccountsAsync().Returns(new List<Account>
        {
            new() { Id = "acc-1", Name = "Girokonto", SystemKey = Finanzuebersicht.Constants.SystemAccountKeys.Default }
        });

        var sut = new LoadRecurringTransactionDetailDataUseCase(categoryRepository, accountRepository);

        var result = await sut.ExecuteAsync("cat-x", "acc-x");

        Assert.Single(result.Kategorien);
        Assert.Null(result.SelectedKategorie);
        Assert.NotNull(result.SelectedAccount);
        Assert.Equal("acc-1", result.SelectedAccount!.Id);
        await categoryRepository.Received(1).GetCategoriesAsync();
        await accountRepository.Received(1).GetAccountsAsync();
    }
}