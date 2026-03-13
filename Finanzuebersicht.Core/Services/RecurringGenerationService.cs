using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class RecurringGenerationService(
    IRecurringTransactionRepository recurringRepository,
    ITransactionRepository transactionRepository) : IRecurringGenerationService
{
    private readonly IRecurringTransactionRepository _recurringRepository = recurringRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task GeneratePendingRecurringTransactionsAsync()
    {
        var recurringItems = await _recurringRepository.GetRecurringTransactionsAsync();
        var today = DateTime.Today;

        foreach (var recurring in recurringItems.Where(item => item.Aktiv))
        {
            if (recurring.Enddatum.HasValue && recurring.Enddatum.Value < today)
                continue;

            var nextMonth = !recurring.LetzteAusfuehrung.HasValue
                ? recurring.Startdatum
                : new DateTime(
                    recurring.LetzteAusfuehrung.Value.Year,
                    recurring.LetzteAusfuehrung.Value.Month,
                    1).AddMonths(1);

            while (nextMonth <= today)
            {
                var transaction = new Transaction
                {
                    Betrag = recurring.Betrag,
                    Titel = recurring.Titel,
                    Datum = nextMonth,
                    KategorieId = recurring.KategorieId,
                    Typ = recurring.Typ,
                    DauerauftragId = recurring.Id
                };

                await _transactionRepository.SaveTransactionAsync(transaction);

                recurring.LetzteAusfuehrung = nextMonth;
                nextMonth = nextMonth.AddMonths(1);
            }

            await _recurringRepository.SaveRecurringTransactionAsync(recurring);
        }
    }
}
