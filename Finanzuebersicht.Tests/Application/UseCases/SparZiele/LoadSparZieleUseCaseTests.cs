using Finanzuebersicht.Application.UseCases.SparZiele;
using Finanzuebersicht.Models;
using Finanzuebersicht.Tests.TestHelpers;

namespace Finanzuebersicht.Tests.Application.UseCases.SparZiele;

public class LoadSparZieleUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_IncludesLinkedTransactionAmountsAndForecast()
    {
        var sparZielRepository = Substitute.For<ISparZielRepository>();
        sparZielRepository.GetSparZieleAsync().Returns(new List<SparZiel>
        {
            new()
            {
                Id = "goal-1",
                Titel = "Urlaub",
                ZielBetrag = 1000m,
                AktuellerBetrag = 100m,
                MonatlicheSparrate = 200m
            }
        });

        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<Transaction>
            {
                new() { SparZielId = "goal-1", Betrag = 150m }
            });

        var clock = new FixedClock(new DateTime(2026, 3, 1));
        var sut = new LoadSparZieleUseCase(sparZielRepository, transactionRepository, clock);

        var result = await sut.ExecuteAsync();
        var summary = Assert.Single(result);

        Assert.Equal(150m, summary.VerknuepfterBetrag);
        Assert.Equal(250m, summary.GesamtFortschritt);
        Assert.NotNull(summary.PrognostiziertesDatum);
        Assert.Equal(new DateTime(2026, 7, 1), summary.PrognostiziertesDatum);
    }
}
