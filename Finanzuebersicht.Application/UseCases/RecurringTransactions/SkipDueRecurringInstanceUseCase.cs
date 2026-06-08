using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.RecurringTransactions;

public class SkipDueRecurringInstanceUseCase(AddRecurringExceptionUseCase addRecurringExceptionUseCase)
{
    private readonly AddRecurringExceptionUseCase _addRecurringExceptionUseCase = addRecurringExceptionUseCase;

    public Task ExecuteAsync(string recurringTransactionId, DateTime instanceDate, CancellationToken cancellationToken = default)
        => _addRecurringExceptionUseCase.ExecuteAsync(
            recurringTransactionId,
            new RecurringException
            {
                InstanceDate = instanceDate.Date,
                Type = RecurringExceptionType.Skip
            },
            cancellationToken);
}
