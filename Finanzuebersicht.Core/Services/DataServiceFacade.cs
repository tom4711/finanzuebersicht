using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class DataServiceFacade : IDataService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRecurringTransactionRepository _recurringRepository;
    private readonly IRecurringGenerationService _recurringGenerationService;
    private readonly IReportingService _reportingService;

    public DataServiceFacade(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringRepository,
        IRecurringGenerationService recurringGenerationService,
        IReportingService reportingService)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _recurringRepository = recurringRepository;
        _recurringGenerationService = recurringGenerationService;
        _reportingService = reportingService;
    }

    public Task<List<Category>> GetCategoriesAsync()
        => _categoryRepository.GetCategoriesAsync();

    public Task SaveCategoryAsync(Category category)
        => _categoryRepository.SaveCategoryAsync(category);

    public Task DeleteCategoryAsync(string id)
        => _categoryRepository.DeleteCategoryAsync(id);

    public Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
        => _transactionRepository.GetTransactionsAsync(vonDatum, bisDatum);

    public Task SaveTransactionAsync(Transaction transaction)
        => _transactionRepository.SaveTransactionAsync(transaction);

    public Task DeleteTransactionAsync(string id)
        => _transactionRepository.DeleteTransactionAsync(id);

    public Task<List<RecurringTransaction>> GetRecurringTransactionsAsync()
        => _recurringRepository.GetRecurringTransactionsAsync();

    public Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
        => _recurringRepository.SaveRecurringTransactionAsync(recurring);

    public Task DeleteRecurringTransactionAsync(string id)
        => _recurringRepository.DeleteRecurringTransactionAsync(id);

    public Task GeneratePendingRecurringTransactionsAsync()
        => _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();

    public Task<YearSummary> GetYearSummaryAsync(int year)
        => _reportingService.GetYearSummaryAsync(year);

    public Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
        => _reportingService.GetMonthSummaryAsync(year, month);

    public Task<Category?> GetMostCommonCategoryForPayeeAsync(
        string payee,
        double confidenceThreshold = 0.5,
        CancellationToken cancellationToken = default)
        => _transactionRepository.GetMostCommonCategoryForPayeeAsync(payee, confidenceThreshold, cancellationToken);
}
