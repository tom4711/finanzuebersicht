using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Application.UseCases.Categories;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Category), "Category")]
public partial class CategoryDetailViewModel(SaveCategoryDetailUseCase saveCategoryDetailUseCase, INavigationService navigationService) : ObservableObject
{
    private readonly SaveCategoryDetailUseCase _saveCategoryDetailUseCase = saveCategoryDetailUseCase;
    private readonly INavigationService _navigationService = navigationService;
    private Category? _existingCategory;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string icon = "💰";

    [ObservableProperty]
    private string color = "#007AFF";

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

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
            }
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        await _saveCategoryDetailUseCase.ExecuteAsync(_existingCategory, Name, Icon, Color, Typ);
        await _navigationService.GoBackAsync();
    }
}
