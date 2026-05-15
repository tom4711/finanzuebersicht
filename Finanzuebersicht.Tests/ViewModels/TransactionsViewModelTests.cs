using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;
using Microsoft.Extensions.Logging;

namespace Finanzuebersicht.Tests.ViewModels;

public class TransactionsViewModelTests
{
    [Fact]
    public async Task ClearSearch_ResetsAllFiltersAndReloads()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(new List<Transaction>()));

        var searchTransactionRepository = Substitute.For<ITransactionRepository>();
        searchTransactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(new List<Transaction>()));

        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync()
            .Returns(Task.FromResult(new List<Category>()));

        var searchCategoryRepository = Substitute.For<ICategoryRepository>();
        searchCategoryRepository.GetCategoriesAsync()
            .Returns(Task.FromResult(new List<Category>()));

        var viewModel = CreateSut(
            transactionRepository,
            categoryRepository,
            searchTransactionRepository,
            searchCategoryRepository,
            out _,
            out _,
            out _,
            out _);

        viewModel.SearchText = "Suche";
        viewModel.SelectedKategorieId = "cat-1";
        viewModel.SelectedTypFilter = TransactionTypeFilter.Ausgabe;
        viewModel.IsDateFilterEnabled = true;
        viewModel.VonDatum = new DateTime(2026, 1, 1);
        viewModel.BisDatum = new DateTime(2026, 1, 31);
        viewModel.SearchErgebnisGruppen = new ObservableCollection<TransactionGroup>
        {
            new(new DateTime(2026, 1, 1), new[] { new Transaction { Id = "t1", Titel = "Test" } })
        };

        await viewModel.ClearSearchCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, viewModel.SearchText);
        Assert.Null(viewModel.SelectedKategorieId);
        Assert.Equal(TransactionTypeFilter.Alle, viewModel.SelectedTypFilter);
        Assert.False(viewModel.IsDateFilterEnabled);
        Assert.Null(viewModel.VonDatum);
        Assert.Null(viewModel.BisDatum);
        Assert.Empty(viewModel.SearchErgebnisGruppen);
        await transactionRepository.Received(1).GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task LoadTransaktionen_WhenAlreadyLoading_DoesNotExecuteAgain()
    {
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish = new TaskCompletionSource<List<Transaction>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(_ =>
            {
                started.TrySetResult(true);
                return finish.Task;
            });

        var searchTransactionRepository = Substitute.For<ITransactionRepository>();
        searchTransactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(new List<Transaction>()));

        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync()
            .Returns(Task.FromResult(new List<Category>()));

        var searchCategoryRepository = Substitute.For<ICategoryRepository>();
        searchCategoryRepository.GetCategoriesAsync()
            .Returns(Task.FromResult(new List<Category>()));

        var viewModel = CreateSut(
            transactionRepository,
            categoryRepository,
            searchTransactionRepository,
            searchCategoryRepository,
            out _,
            out _,
            out _,
            out _);

        var firstLoad = viewModel.LoadTransaktionenCommand.ExecuteAsync(null);
        await started.Task;

        var secondLoad = viewModel.LoadTransaktionenCommand.ExecuteAsync(null);

        finish.SetResult(new List<Transaction>());

        await Task.WhenAll(firstLoad, secondLoad);

        await transactionRepository.Received(1).GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task DeleteTransaktion_WhenConfirmed_CallsDeleteUseCase()
    {
        var deleteRepository = Substitute.For<ITransactionRepository>();
        deleteRepository.DeleteTransactionAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var viewModel = CreateSut(
            Substitute.For<ITransactionRepository>(),
            Substitute.For<ICategoryRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<ICategoryRepository>(),
            out var dialogService,
            out _,
            out _,
            out _,
            deleteRepository);

        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(true));

        var transaction = new Transaction { Id = "tx-1", Titel = "Miete" };

        await viewModel.DeleteTransaktionCommand.ExecuteAsync(transaction);

        await deleteRepository.Received(1).DeleteTransactionAsync("tx-1");
    }

    [Fact]
    public async Task DeleteTransaktion_WhenNotConfirmed_DoesNotCallDeleteUseCase()
    {
        var deleteRepository = Substitute.For<ITransactionRepository>();

        var viewModel = CreateSut(
            Substitute.For<ITransactionRepository>(),
            Substitute.For<ICategoryRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<ICategoryRepository>(),
            out var dialogService,
            out _,
            out _,
            out _,
            deleteRepository);

        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        var transaction = new Transaction { Id = "tx-1", Titel = "Miete" };

        await viewModel.DeleteTransaktionCommand.ExecuteAsync(transaction);

        await deleteRepository.DidNotReceive().DeleteTransactionAsync(Arg.Any<string>());
    }

    private static TransactionsViewModel CreateSut(
        ITransactionRepository loadTransactionRepository,
        ICategoryRepository loadCategoryRepository,
        ITransactionRepository searchTransactionRepository,
        ICategoryRepository searchCategoryRepository,
        out IDialogService dialogService,
        out ILocalizationService localizationService,
        out IMainThreadDispatcher dispatcher,
        out INavigationService navigationService,
        ITransactionRepository? deleteTransactionRepository = null)
    {
        dialogService = Substitute.For<IDialogService>();
        dialogService.ShowAlertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString(Arg.Any<string>()).Returns(call => call.Arg<string>());
        localizationService.GetString(Arg.Any<string>(), Arg.Any<object[]>()).Returns(call => call.Arg<string>());

        dispatcher = Substitute.For<IMainThreadDispatcher>();
        dispatcher.InvokeAsync(Arg.Any<Func<Task>>())
            .Returns(call => call.Arg<Func<Task>>()());

        navigationService = Substitute.For<INavigationService>();

        var filePicker = Substitute.For<IFilePicker>();
        var appEvents = Substitute.For<IAppEvents>();
        var logger = Substitute.For<ILogger<TransactionsViewModel>>();
        var importLogger = Substitute.For<ILogger<ImportService>>();
        var importTransactionRepository = Substitute.For<ITransactionRepository>();
        var importCategoryRepository = Substitute.For<ICategoryRepository>();
        var importService = new ImportService([], importTransactionRepository, importLogger, importCategoryRepository);

        deleteTransactionRepository ??= Substitute.For<ITransactionRepository>();
        deleteTransactionRepository.DeleteTransactionAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        return new TransactionsViewModel(
            new DeleteTransactionUseCase(deleteTransactionRepository),
            new LoadTransactionsMonthUseCase(loadTransactionRepository, loadCategoryRepository),
            new SearchTransactionsUseCase(searchTransactionRepository, searchCategoryRepository),
            navigationService,
            importService,
            dialogService,
            localizationService,
            loadCategoryRepository,
            dispatcher,
            filePicker,
            appEvents,
            logger);
    }
}
