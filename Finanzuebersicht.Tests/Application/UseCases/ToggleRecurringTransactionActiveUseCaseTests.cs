using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class ToggleRecurringTransactionActiveUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_TogglesActiveAndSaves()
    {
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var recurring = new RecurringTransaction { Id = "r-1", Aktiv = false };
        var sut = new ToggleRecurringTransactionActiveUseCase(recurringRepository);

        await sut.ExecuteAsync(recurring);

        Assert.True(recurring.Aktiv);
        await recurringRepository.Received(1).SaveRecurringTransactionAsync(recurring);
    }
}