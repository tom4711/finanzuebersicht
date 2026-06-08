using Finanzuebersicht.Application.UseCases.Accounts;
using Finanzuebersicht.Application.UseCases.Dashboard;
using Finanzuebersicht.Application.UseCases.RecurringTransactions;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Models;
using Finanzuebersicht.Tests.TestHelpers;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.ViewModels;

public class DashboardViewModelTests
{
    [Fact]
    public void HasYearData_WhenJahrGruppenEmpty_ReturnsFalse()
    {
        var viewModel = CreateSut();

        Assert.False(viewModel.HasYearData);
    }

    [Fact]
    public void HasMonthData_WhenCollectionsEmpty_ReturnsFalse()
    {
        var viewModel = CreateSut();

        Assert.False(viewModel.HasMonthData);
    }

    private static DashboardViewModel CreateSut()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var recurringTransactionRepository = Substitute.For<IRecurringTransactionRepository>();
        var budgetRepository = Substitute.For<IBudgetRepository>();
        var reportingService = Substitute.For<IReportingService>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.GetAccountsAsync().Returns(Task.FromResult(new List<Account>()));
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(new List<Transaction>()));
        var localizationService = Substitute.For<ILocalizationService>();
        var navigationService = Substitute.For<INavigationService>();
        var forecastService = Substitute.For<IForecastService>();
        var clock = new FixedClock(new DateTime(2026, 3, 15));

        return new DashboardViewModel(
            new LoadDashboardMonthUseCase(categoryRepository, transactionRepository, recurringTransactionRepository, budgetRepository),
            new LoadDashboardYearUseCase(transactionRepository, categoryRepository),
            new LoadForecastUseCase(forecastService),
            new GetDueRecurringWithHintsUseCase(recurringTransactionRepository),
            new BookDueRecurringInstanceUseCase(recurringTransactionRepository, transactionRepository, accountRepository),
            new SkipDueRecurringInstanceUseCase(new AddRecurringExceptionUseCase(recurringTransactionRepository)),
            budgetRepository,
            localizationService,
            navigationService,
            Substitute.For<IDialogService>(),
            transactionRepository,
            accountRepository,
            new GetAccountBalancesUseCase(accountRepository, transactionRepository),
            clock);
    }
}
