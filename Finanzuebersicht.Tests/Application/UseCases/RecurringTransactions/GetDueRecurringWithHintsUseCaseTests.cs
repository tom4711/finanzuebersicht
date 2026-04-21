using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases.RecurringTransactions;

public class GetDueRecurringWithHintsUseCaseTests
{
    private static GetDueRecurringWithHintsUseCase CreateSut(params RecurringTransaction[] transactions)
    {
        var repo = Substitute.For<IRecurringTransactionRepository>();
        repo.GetRecurringTransactionsAsync().Returns(transactions.ToList());
        return new GetDueRecurringWithHintsUseCase(repo);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHint_WhenStartDateIsInFutureButWithinReminderWindow()
    {
        var today = new DateTime(2026, 4, 21);
        var startDate = new DateTime(2026, 4, 23); // 2 days in the future
        var recurring = new RecurringTransaction
        {
            Id = "r-1",
            Aktiv = true,
            Startdatum = startDate,
            Interval = RecurrenceInterval.Monthly,
            IntervalFactor = 1,
            ReminderDaysBefore = 2
        };
        var sut = CreateSut(recurring);

        var result = await sut.ExecuteAsync(today);

        Assert.Single(result);
        Assert.NotNull(result[0].Hint);
        Assert.Contains("2", result[0].Hint); // "Fällig in 2 Tagen"
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNoHint_WhenStartDateIsInFutureAndOutsideReminderWindow()
    {
        var today = new DateTime(2026, 4, 21);
        var startDate = new DateTime(2026, 4, 25); // 4 days in the future
        var recurring = new RecurringTransaction
        {
            Id = "r-1",
            Aktiv = true,
            Startdatum = startDate,
            Interval = RecurrenceInterval.Monthly,
            IntervalFactor = 1,
            ReminderDaysBefore = 2
        };
        var sut = CreateSut(recurring);

        var result = await sut.ExecuteAsync(today);

        // Filtered out: start date is beyond the reminder window
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHint_WhenDueToday()
    {
        var today = new DateTime(2026, 4, 21);
        var recurring = new RecurringTransaction
        {
            Id = "r-1",
            Aktiv = true,
            Startdatum = today,
            Interval = RecurrenceInterval.Monthly,
            IntervalFactor = 1,
            ReminderDaysBefore = 0
        };
        var sut = CreateSut(recurring);

        var result = await sut.ExecuteAsync(today);

        Assert.Single(result);
        Assert.Equal("Heute fällig", result[0].Hint);
        Assert.True(result[0].IsDue);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHint_WhenLetzteAusfuehrungSetAndNextInstanceWithinReminderWindow()
    {
        var today = new DateTime(2026, 4, 21);
        var recurring = new RecurringTransaction
        {
            Id = "r-1",
            Aktiv = true,
            Startdatum = new DateTime(2026, 1, 1),
            Interval = RecurrenceInterval.Weekly,
            IntervalFactor = 1,
            ReminderDaysBefore = 7,
            LetzteAusfuehrung = new DateTime(2026, 4, 19) // next = 2026-04-26, 5 days away
        };
        var sut = CreateSut(recurring);

        var result = await sut.ExecuteAsync(today);

        Assert.Single(result);
        Assert.NotNull(result[0].Hint);
        Assert.Contains("5", result[0].Hint); // "Fällig in 5 Tagen"
        Assert.False(result[0].IsDue);
    }

    [Fact]
    public async Task ExecuteAsync_ExcludesInactiveTransactions()
    {
        var today = new DateTime(2026, 4, 21);
        var recurring = new RecurringTransaction
        {
            Id = "r-1",
            Aktiv = false,
            Startdatum = today,
            Interval = RecurrenceInterval.Monthly,
            IntervalFactor = 1
        };
        var sut = CreateSut(recurring);

        var result = await sut.ExecuteAsync(today);

        Assert.Empty(result);
    }
}
