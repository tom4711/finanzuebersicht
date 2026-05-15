using System.Collections.ObjectModel;
using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Models;
using Finanzuebersicht.Navigation;
using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.ViewModels;

public class CategoriesViewModelTests
{
    [Fact]
    public async Task LoadKategorien_PopulatesKategorien()
    {
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(Task.FromResult(new List<Category>
        {
            new() { Id = "cat-1", Name = "Miete" },
            new() { Id = "cat-2", Name = "Lebensmittel" }
        }));

        var sut = CreateSut(
            categoryRepository,
            Substitute.For<ITransactionRepository>(),
            Substitute.For<IRecurringTransactionRepository>(),
            out _,
            out _);

        await sut.LoadKategorienCommand.ExecuteAsync(null);

        Assert.Collection(
            sut.Kategorien,
            item => Assert.Equal("cat-1", item.Id),
            item => Assert.Equal("cat-2", item.Id));
    }

    [Fact]
    public async Task DeleteKategorie_WhenConfirmed_CallsDeleteUseCase()
    {
        var categoryToDelete = new Category { Id = "cat-1", Name = "Miete" };
        var fallbackCategory = new Category { Id = "cat-2", Name = "Sonstiges" };
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.GetCategoriesAsync().Returns(Task.FromResult(new List<Category> { categoryToDelete, fallbackCategory }));
        categoryRepository.DeleteCategoryAsync("cat-1").Returns(Task.CompletedTask);

        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(new List<Transaction>()));

        var recurringTransactionRepository = Substitute.For<IRecurringTransactionRepository>();
        recurringTransactionRepository.GetRecurringTransactionsAsync()
            .Returns(Task.FromResult(new List<RecurringTransaction>()));

        var sut = CreateSut(
            categoryRepository,
            transactionRepository,
            recurringTransactionRepository,
            out var dialogService,
            out _);

        sut.Kategorien = new ObservableCollection<Category> { categoryToDelete };
        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(true));

        await sut.DeleteKategorieCommand.ExecuteAsync(categoryToDelete);

        await categoryRepository.Received(1).DeleteCategoryAsync("cat-1");
        Assert.Empty(sut.Kategorien);
    }

    [Fact]
    public async Task DeleteKategorie_WhenNotConfirmed_DoesNotDelete()
    {
        var categoryToDelete = new Category { Id = "cat-1", Name = "Miete" };
        var sut = CreateSut(
            Substitute.For<ICategoryRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<IRecurringTransactionRepository>(),
            out var dialogService,
            out var categoryRepository);

        sut.Kategorien = new ObservableCollection<Category> { categoryToDelete };
        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        await sut.DeleteKategorieCommand.ExecuteAsync(categoryToDelete);

        await categoryRepository.DidNotReceive().DeleteCategoryAsync(Arg.Any<string>());
        Assert.Single(sut.Kategorien);
    }

    [Fact]
    public async Task GoToDetail_NavigatesToCategoryDetailRoute()
    {
        var category = new Category { Id = "cat-1", Name = "Miete" };
        var sut = CreateSut(
            Substitute.For<ICategoryRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<IRecurringTransactionRepository>(),
            out _,
            out _,
            out var navigationService);

        await sut.GoToDetailCommand.ExecuteAsync(category);

        await navigationService.Received(1).GoToAsync(
            Routes.CategoryDetail,
            Arg.Is<IDictionary<string, object>?>(parameters =>
                parameters != null &&
                parameters.ContainsKey("Category") &&
                object.ReferenceEquals(parameters["Category"], category)));
    }

    private static CategoriesViewModel CreateSut(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringTransactionRepository,
        out IDialogService dialogService,
        out ICategoryRepository deleteCategoryRepository,
        out INavigationService navigationService)
    {
        dialogService = Substitute.For<IDialogService>();
        dialogService.ShowAlertAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
        dialogService.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString(Arg.Any<string>()).Returns(call => call.Arg<string>());
        localizationService.GetString(Arg.Any<string>(), Arg.Any<object[]>()).Returns(call => call.ArgAt<string>(0));

        navigationService = Substitute.For<INavigationService>();
        deleteCategoryRepository = categoryRepository;

        return new CategoriesViewModel(
            new DeleteCategoryUseCase(categoryRepository, transactionRepository, recurringTransactionRepository),
            new LoadCategoriesUseCase(categoryRepository),
            localizationService,
            navigationService,
            dialogService);
    }

    private static CategoriesViewModel CreateSut(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringTransactionRepository,
        out IDialogService dialogService,
        out ICategoryRepository deleteCategoryRepository)
    {
        return CreateSut(
            categoryRepository,
            transactionRepository,
            recurringTransactionRepository,
            out dialogService,
            out deleteCategoryRepository,
            out _);
    }
}
