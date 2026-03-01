using Finanzuebersicht.ViewModels;
using Finanzuebersicht.Converters;
using Finanzuebersicht.Services;
using Microsoft.Maui.Controls;

namespace Finanzuebersicht.Views;

public partial class TransactionsPage : ContentPage
{
    private readonly IDataService _dataService;

    public TransactionsPage(TransactionsViewModel viewModel, IDataService dataService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _dataService = dataService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is TransactionsViewModel vm)
        {
            vm.LoadTransaktionenCommand.Execute(null);
            
            // Load categories and populate converter cache
            try
            {
                var categories = await _dataService.GetCategoriesAsync();
                var categoryIconMap = categories.ToDictionary(c => c.Id, c => c.Icon ?? "📁");
                KategorieIdToIconConverter.SetCache(categoryIconMap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories for icons: {ex.Message}");
            }
        }
    }
}
