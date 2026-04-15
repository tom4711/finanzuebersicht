using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;
using System.Globalization;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Category), "Category")]
public partial class CategoryDetailViewModel(
    SaveCategoryDetailUseCase saveCategoryDetailUseCase,
    INavigationService navigationService,
    SaveCategoryBudgetUseCase? saveCategoryBudgetUseCase = null,
    IBudgetRepository? budgetRepository = null) : ObservableObject
{
    private readonly SaveCategoryDetailUseCase _saveCategoryDetailUseCase = saveCategoryDetailUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private readonly SaveCategoryBudgetUseCase? _saveCategoryBudgetUseCase = saveCategoryBudgetUseCase;
    private readonly IBudgetRepository? _budgetRepository = budgetRepository;
    private Category? _existingCategory;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string icon = "💰";

    [ObservableProperty]
    private string color = "#007AFF";

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

    // String-backed property for culture-safe decimal input
    [ObservableProperty]
    private string monthlyBudgetText = string.Empty;

    public string PageTitle => _existingCategory == null 
        ? LocalizationResourceManager.Current[ResourceKeys.Title_NeueKategorie] 
        : LocalizationResourceManager.Current[ResourceKeys.Title_KategorieBearbeiten];

    public List<string> VerfuegbareIcons { get; } =
    [
        "🛒", "🚗", "🏠", "🎬", "💊", "💼", "📦",
        "🍽️", "☕", "🎮", "📱", "✈️", "🎓", "💰",
        "🏋️", "🎁", "👕", "🔧", "📰", "🐾"
    ];

    public List<string> VerfuegbareFarben { get; } =
    [
        "#007AFF", "#34C759", "#FF3B30", "#FF9500",
        "#AF52DE", "#5856D6", "#FF2D55", "#A2845E",
        "#00C7BE", "#32ADE6"
    ];

    public Category? Category
    {
        set
        {
            if (value != null)
            {
                _existingCategory = value;
                Name = value.Name;
                Icon = value.Icon;
                Color = value.Color;
                Typ = value.Typ;
                _ = LoadBudgetAsync(value.Id);
            }
        }
    }

    private async Task LoadBudgetAsync(string kategorieId)
    {
        try
        {
            if (_budgetRepository == null || string.IsNullOrEmpty(kategorieId)) return;
            // Always load the default budget (monat=null, jahr=null) — consistent with Save
            var budgets = await _budgetRepository.GetBudgetsAsync();
            var budget = budgets.FirstOrDefault(b => b.KategorieId == kategorieId && b.Monat == null && b.Jahr == null);
            var betrag = budget?.Betrag ?? 0;
            MonthlyBudgetText = betrag > 0 ? betrag.ToString("F2", CultureInfo.CurrentCulture) : string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryDetailViewModel] LoadBudgetAsync failed: {ex}");
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var savedCategory = await _saveCategoryDetailUseCase.ExecuteAsync(_existingCategory, Name, Icon, Color, Typ);
        if (_saveCategoryBudgetUseCase != null && !string.IsNullOrEmpty(savedCategory.Id))
        {
            decimal.TryParse(MonthlyBudgetText, NumberStyles.Any, CultureInfo.CurrentCulture, out var budget);
            await _saveCategoryBudgetUseCase.ExecuteAsync(savedCategory.Id, budget);
        }
        await _navigationService.GoBackAsync();
    }
}
