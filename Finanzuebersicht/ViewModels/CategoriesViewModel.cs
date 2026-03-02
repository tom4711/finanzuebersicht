using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using Finanzuebersicht.Views;
using Finanzuebersicht.Resources.Strings;

namespace Finanzuebersicht.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ILocalizationService _loc;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    private bool isLoading;

    public CategoriesViewModel(IDataService dataService, ILocalizationService localizationService)
    {
        _dataService = dataService;
        _loc = localizationService;
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
        var confirm = await Shell.Current.DisplayAlert(
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschen),
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschenFrage, kategorie.Name),
            _loc.GetString(ResourceKeys.Btn_Ja), _loc.GetString(ResourceKeys.Btn_Nein));
        if (!confirm) return;

        await _dataService.DeleteCategoryAsync(kategorie.Id);
        Kategorien.Remove(kategorie);
    }

    [RelayCommand]
    private async Task GoToDetail(Category? kategorie = null)
    {
        var parameter = new Dictionary<string, object>();
        if (kategorie != null)
            parameter["Category"] = kategorie;

        await Shell.Current.GoToAsync(nameof(CategoryDetailPage), parameter);
    }
}
