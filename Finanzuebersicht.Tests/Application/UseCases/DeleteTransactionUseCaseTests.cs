using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class DeleteTransactionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var sut = new DeleteTransactionUseCase(transactionRepository);

        await sut.ExecuteAsync("tx-1");

        await transactionRepository.Received(1).DeleteTransactionAsync("tx-1");
    }
}