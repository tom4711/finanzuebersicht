using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.ViewModels;

[QueryProperty(nameof(Category), "Category")]
public partial class CategoryDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private Category? _existingCategory;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string icon = "💰";

    [ObservableProperty]
    private string color = "#007AFF";

    [ObservableProperty]
    private TransactionType typ = TransactionType.Ausgabe;

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

    public CategoryDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var category = _existingCategory ?? new Category();
        category.Name = Name;
        category.Icon = Icon;
        category.Color = Color;
        category.Typ = Typ;

        await _dataService.SaveCategoryAsync(category);
        await Shell.Current.GoToAsync("..");
    }
}
