using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

internal static class SparZielProgressCalculator
{
    public static Dictionary<string, decimal> SumLinkedAmounts(IEnumerable<Transaction> transactions)
        => transactions
            .Where(t => !string.IsNullOrWhiteSpace(t.SparZielId))
            .GroupBy(t => t.SparZielId!)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Betrag)));

    public static DateTime? ForecastCompletionDate(SparZiel sparZiel, decimal gesamtFortschritt, DateTime referenceDate)
    {
        if (sparZiel.ZielBetrag <= gesamtFortschritt)
            return referenceDate;

        var remaining = sparZiel.ZielBetrag - gesamtFortschritt;
        if (sparZiel.MonatlicheSparrate is > 0)
        {
            var months = (int)Math.Ceiling(remaining / sparZiel.MonatlicheSparrate.Value);
            return referenceDate.Date.AddMonths(months);
        }

        return sparZiel.Faelligkeitsdatum;
    }
}
