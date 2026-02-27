using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;

namespace Finanzuebersicht.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    private bool isLoading;

    public CategoriesViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _dataService.GetCategoriesAsync();
            Kategorien = new ObservableCollection<Category>(liste);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteKategorie(Category kategorie)
    {
        await _dataService.DeleteCategoryAsync(kategorie.Id);
        Kategorien.Remove(kategorie);
    }

    [RelayCommand]
    private async Task GoToDetail(Category? kategorie)
    {
        var parameter = new Dictionary<string, object>();
        if (kategorie != null)
            parameter["Category"] = kategorie;

        await Shell.Current.GoToAsync(nameof(CategoryDetailPage), parameter);
    }
}
