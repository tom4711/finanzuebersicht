using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class RecurringGenerationServiceTests
{
    [Fact]
    public async Task GeneratePendingRecurringTransactionsAsync_CreatesExpectedTransactions()
    {
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();

        var recurring = new RecurringTransaction
        {
            Id = Guid.NewGuid().ToString(),
            Titel = "Abo",
            Betrag = 12m,
            Typ = TransactionType.Ausgabe,
            Startdatum = DateTime.Today.AddMonths(-2),
            Aktiv = true,
            KategorieId = "cat-1"
        };

        recurringRepository.GetRecurringTransactionsAsync()
            .Returns(new List<RecurringTransaction> { recurring });

        var service = new RecurringGenerationService(recurringRepository, transactionRepository);

        await service.GeneratePendingRecurringTransactionsAsync();

        var savedTransactions = transactionRepository
            .ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITransactionRepository.SaveTransactionAsync))
            .Select(call => (Transaction)call.GetArguments()[0])
            .ToList();

        Assert.True(savedTransactions.Count >= 2);
        Assert.All(savedTransactions, t =>
        {
            Assert.Equal(recurring.Id, t.DauerauftragId);
            Assert.Equal("Abo", t.Titel);
            Assert.Equal(12m, t.Betrag);
        });

        await recurringRepository.Received(1).SaveRecurringTransactionAsync(Arg.Is<RecurringTransaction>(r =>
            r.Id == recurring.Id && r.LetzteAusfuehrung.HasValue));
    }

    [Fact]
    public async Task GeneratePendingRecurringTransactionsAsync_IgnoresInactiveItems()
    {
        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();

        recurringRepository.GetRecurringTransactionsAsync()
            .Returns(new List<RecurringTransaction>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Titel = "Inaktiv",
                    Betrag = 5m,
                    Typ = TransactionType.Ausgabe,
                    Startdatum = DateTime.Today.AddMonths(-3),
                    Aktiv = false,
                    KategorieId = "cat-1"
                }
            });

        var service = new RecurringGenerationService(recurringRepository, transactionRepository);

        await service.GeneratePendingRecurringTransactionsAsync();

        await transactionRepository.DidNotReceiveWithAnyArgs().SaveTransactionAsync(default!);
    }
}
