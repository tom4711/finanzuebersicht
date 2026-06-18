using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Tests.Application.UseCases.Transactions;

public class HasAnyTransactionsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsFalse_WhenNoTransactions()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>());

        var sut = new HasAnyTransactionsUseCase(transactionRepository);
        Assert.False(await sut.ExecuteAsync());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTrue_WhenTransactionsExist()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction> { new() { Betrag = 10m } });

        var sut = new HasAnyTransactionsUseCase(transactionRepository);
        Assert.True(await sut.ExecuteAsync());
    }
}
