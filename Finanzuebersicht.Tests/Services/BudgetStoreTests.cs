using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services;

public class BudgetStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly BudgetStore _store;

    public BudgetStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"budget_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _store = new BudgetStore(_testDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_testDir, true); } catch { }
    }

    [Fact]
    public async Task SaveAndGet_ReturnsSavedBudget()
    {
        var budget = new CategoryBudget
        {
            KategorieId = "cat-1",
            Betrag = 500m
        };

        await _store.SaveBudgetAsync(budget);
        var budgets = await _store.GetBudgetsAsync();

        Assert.Single(budgets, b => b.Id == budget.Id && b.Betrag == 500m);
    }

    [Fact]
    public async Task Update_ReplacesExistingBudget()
    {
        var budget = new CategoryBudget { KategorieId = "cat-1", Betrag = 300m };
        await _store.SaveBudgetAsync(budget);

        budget.Betrag = 400m;
        await _store.SaveBudgetAsync(budget);

        var budgets = await _store.GetBudgetsAsync();
        Assert.Single(budgets);
        Assert.Equal(400m, budgets[0].Betrag);
    }

    [Fact]
    public async Task Delete_RemovesBudget()
    {
        var budget = new CategoryBudget { KategorieId = "cat-1", Betrag = 200m };
        await _store.SaveBudgetAsync(budget);

        await _store.DeleteBudgetAsync(budget.Id);

        var budgets = await _store.GetBudgetsAsync();
        Assert.Empty(budgets);
    }

    [Fact]
    public async Task GetBudgetForCategory_PrefersSpecificYearMonth()
    {
        var defaultBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 100m, Monat = null, Jahr = null };
        var monthOnlyBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 200m, Monat = 3, Jahr = null };
        var specificBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 300m, Monat = 3, Jahr = 2025 };

        await _store.SaveBudgetAsync(defaultBudget);
        await _store.SaveBudgetAsync(monthOnlyBudget);
        await _store.SaveBudgetAsync(specificBudget);

        var result = await _store.GetBudgetForCategoryAsync("cat-1", 2025, 3);

        Assert.NotNull(result);
        Assert.Equal(300m, result.Betrag);
    }

    [Fact]
    public async Task GetBudgetForCategory_FallsBackToMonthOnly()
    {
        var defaultBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 100m, Monat = null, Jahr = null };
        var monthOnlyBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 200m, Monat = 3, Jahr = null };

        await _store.SaveBudgetAsync(defaultBudget);
        await _store.SaveBudgetAsync(monthOnlyBudget);

        var result = await _store.GetBudgetForCategoryAsync("cat-1", 2025, 3);

        Assert.NotNull(result);
        Assert.Equal(200m, result.Betrag);
    }

    [Fact]
    public async Task GetBudgetForCategory_FallsBackToDefault()
    {
        var defaultBudget = new CategoryBudget { KategorieId = "cat-1", Betrag = 100m, Monat = null, Jahr = null };
        await _store.SaveBudgetAsync(defaultBudget);

        var result = await _store.GetBudgetForCategoryAsync("cat-1", 2025, 3);

        Assert.NotNull(result);
        Assert.Equal(100m, result.Betrag);
    }

    [Fact]
    public async Task GetBudgetForCategory_ReturnsNull_WhenNoBudgetExists()
    {
        var result = await _store.GetBudgetForCategoryAsync("cat-1", 2025, 3);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBudgetForCategory_DoesNotReturnBudgetForDifferentCategory()
    {
        var budget = new CategoryBudget { KategorieId = "cat-2", Betrag = 150m, Monat = null, Jahr = null };
        await _store.SaveBudgetAsync(budget);

        var result = await _store.GetBudgetForCategoryAsync("cat-1", 2025, 3);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReplaceAll_ReplacesExistingBudgetsWithNewList()
    {
        var existing = new CategoryBudget { KategorieId = "cat-old", Betrag = 100m };
        await _store.SaveBudgetAsync(existing);

        var newBudgets = new List<CategoryBudget>
        {
            new() { KategorieId = "cat-a", Betrag = 200m },
            new() { KategorieId = "cat-b", Betrag = 300m },
        };

        await _store.ReplaceAllBudgetsAsync(newBudgets);

        var result = await _store.GetBudgetsAsync();
        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Contains(b.KategorieId, new[] { "cat-a", "cat-b" }));
        Assert.DoesNotContain(result, b => b.KategorieId == "cat-old");
    }

    [Fact]
    public async Task ReplaceAll_WithEmptyList_ClearsAllBudgets()
    {
        await _store.SaveBudgetAsync(new CategoryBudget { KategorieId = "cat-1", Betrag = 100m });
        await _store.SaveBudgetAsync(new CategoryBudget { KategorieId = "cat-2", Betrag = 200m });

        await _store.ReplaceAllBudgetsAsync([]);

        var result = await _store.GetBudgetsAsync();
        Assert.Empty(result);
    }
}
