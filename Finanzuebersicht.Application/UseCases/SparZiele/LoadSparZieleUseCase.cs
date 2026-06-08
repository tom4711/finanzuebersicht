using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class LoadSparZieleUseCase(
    ISparZielRepository sparZielRepository,
    ITransactionRepository transactionRepository,
    Finanzuebersicht.Core.Services.IClock? clock = null)
{
    private readonly Finanzuebersicht.Core.Services.IClock _clock = clock ?? Finanzuebersicht.Core.Services.SystemClock.Instance;

    public async Task<List<SparZielSummary>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ziele = await sparZielRepository.GetSparZieleAsync();
        var transactions = await transactionRepository.GetTransactionsAsync(
            new DateTime(1900, 1, 1),
            new DateTime(2100, 12, 31, 23, 59, 59));
        var linked = SparZielProgressCalculator.SumLinkedAmounts(transactions);
        var today = _clock.Today;

        return ziele.Select(z =>
        {
            linked.TryGetValue(z.Id, out var verknuepft);
            var gesamt = z.AktuellerBetrag + verknuepft;
            return new SparZielSummary
            {
                SparZiel = z,
                VerknuepfterBetrag = verknuepft,
                PrognostiziertesDatum = SparZielProgressCalculator.ForecastCompletionDate(z, gesamt, today)
            };
        }).ToList();
    }
}
