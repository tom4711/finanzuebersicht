using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class DeleteRecurringTransactionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var sut = new DeleteRecurringTransactionUseCase(recurringRepository);

        await sut.ExecuteAsync("r-1");

        await recurringRepository.Received(1).DeleteRecurringTransactionAsync("r-1");
    }
}