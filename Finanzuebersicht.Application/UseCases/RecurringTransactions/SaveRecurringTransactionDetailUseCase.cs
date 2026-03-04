using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class SaveRecurringTransactionDetailUseCase
{
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;
    private readonly IRecurringGenerationService _recurringGenerationService;

    public SaveRecurringTransactionDetailUseCase(
        IRecurringTransactionRepository recurringTransactionRepository,
        IRecurringGenerationService recurringGenerationService)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
        _recurringGenerationService = recurringGenerationService;
    }

    public async Task ExecuteAsync(
        RecurringTransaction? existing,
        decimal betrag,
        string titel,
        string kategorieId,
        TransactionType typ,
        DateTime startdatum,
        DateTime? enddatum,
        bool aktiv)
    {
        var recurring = existing ?? new RecurringTransaction();
        recurring.Betrag = betrag;
        recurring.Titel = titel;
        recurring.KategorieId = kategorieId;
        recurring.Typ = typ;
        recurring.Startdatum = startdatum;
        recurring.Enddatum = enddatum;
        recurring.Aktiv = aktiv;

        await _recurringTransactionRepository.SaveRecurringTransactionAsync(recurring);
        await _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();
    }
}