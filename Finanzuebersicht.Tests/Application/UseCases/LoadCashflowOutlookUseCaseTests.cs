using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Models;
using Finanzuebersicht.Tests.TestHelpers;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class LoadCashflowOutlookUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_IncludesTransactionsAndProjectedRecurring()
    {
        var today = new DateTime(2026, 3, 15);
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new()
                {
                    Titel = "Gehalt",
                    Betrag = 3000m,
                    Typ = TransactionType.Einnahme,
                    Datum = new DateTime(2026, 3, 20),
                    AccountId = "acc-1"
                }
            });

        var recurringRepository = Substitute.For<IRecurringTransactionRepository>();
        recurringRepository.GetRecurringTransactionsAsync().Returns(new List<RecurringTransaction>
        {
            new()
            {
                Id = "rec-1",
                Titel = "Miete",
                Betrag = 900m,
                Typ = TransactionType.Ausgabe,
                Aktiv = true,
                Startdatum = new DateTime(2026, 1, 1),
                Interval = RecurrenceInterval.Monthly,
                AccountId = "acc-1"
            }
        });

        var sut = new LoadCashflowOutlookUseCase(transactionRepository, recurringRepository, new FixedClock(today));
        var result = await sut.ExecuteAsync(accountId: "acc-1");

        Assert.Contains(result.Days, d => d.Entries.Any(e => e.Title == "Gehalt" && !e.IsProjected));
        Assert.Contains(result.Days, d => d.Entries.Any(e => e.Title == "Miete" && e.IsProjected));
        Assert.Equal(3000m, result.ProjectedIncome);
    }
}
