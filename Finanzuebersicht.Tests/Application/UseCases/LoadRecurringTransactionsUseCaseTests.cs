using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadRecurringTransactionsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSortedRecurringTransactions()
    {
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        recurringRepository.GetRecurringTransactionsAsync().Returns(new List<RecurringTransaction>
        {
            new() { Id = "r2", Titel = "Streaming", Aktiv = false },
            new() { Id = "r3", Titel = "Auto", Aktiv = true },
            new() { Id = "r1", Titel = "Miete", Aktiv = true }
        });

        var sut = new LoadRecurringTransactionsUseCase(recurringRepository);

        var result = await sut.ExecuteAsync();

        Assert.Collection(result,
            x => Assert.Equal("r3", x.Id),
            x => Assert.Equal("r1", x.Id),
            x => Assert.Equal("r2", x.Id));
        await recurringRepository.Received(1).GetRecurringTransactionsAsync();
    }
}