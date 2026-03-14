using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class SaveRecurringTransactionDetailUseCase(
    IRecurringTransactionRepository recurringTransactionRepository,
    IRecurringGenerationService recurringGenerationService)
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository = recurringTransactionRepository;
    private readonly IRecurringGenerationService _recurringGenerationService = recurringGenerationService;

    public async Task ExecuteAsync(
        RecurringTransaction? existing,
        decimal betrag,
        string titel,
        string kategorieId,
        TransactionType typ,
        DateTime startdatum,
        DateTime? enddatum,
        bool aktiv,
        RecurrenceInterval interval = RecurrenceInterval.Monthly,
        int intervalFactor = 1,
        int reminderDaysBefore = 0,
        List<RecurringException>? exceptions = null)
    {
        System.Diagnostics.Debug.WriteLine($"SaveRecurringTransactionDetailUseCase.ExecuteAsync: incoming interval={interval}, intervalFactor={intervalFactor}, titel={titel}, existingId={existing?.Id}");

        var recurring = existing ?? new RecurringTransaction();
        recurring.Betrag = betrag;
        recurring.Titel = titel;
        recurring.KategorieId = kategorieId;
        recurring.Typ = typ;
        recurring.Startdatum = startdatum;
        recurring.Enddatum = enddatum;
        recurring.Aktiv = aktiv;
        recurring.Interval = interval;
        recurring.IntervalFactor = intervalFactor;
        recurring.ReminderDaysBefore = reminderDaysBefore;
        if (exceptions != null)
            recurring.Exceptions = exceptions;

        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
        await _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();
    }
}