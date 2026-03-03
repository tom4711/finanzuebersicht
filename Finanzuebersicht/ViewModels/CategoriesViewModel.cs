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
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILocalizationService _loc;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Category> kategorien = [];

    [ObservableProperty]
    private bool isLoading;

    public CategoriesViewModel(
        ICategoryRepository categoryRepository,
        ILocalizationService localizationService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _categoryRepository = categoryRepository;
        _loc = localizationService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadKategorien()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var liste = await _categoryRepository.GetCategoriesAsync();
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
        var confirm = await _dialogService.ShowConfirmationAsync(
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschen),
            _loc.GetString(ResourceKeys.Dlg_KategorieLoeschenFrage, kategorie.Name),
            _loc.GetString(ResourceKeys.Btn_Ja), _loc.GetString(ResourceKeys.Btn_Nein));
        if (!confirm) return;

        await _categoryRepository.DeleteCategoryAsync(kategorie.Id);
        Kategorien.Remove(kategorie);
    }

    [RelayCommand]
    private async Task GoToDetail(Category? kategorie = null)
    {
        var parameter = new Dictionary<string, object>();
        if (kategorie != null)
            parameter["Category"] = kategorie;

        await _navigationService.GoToAsync(nameof(CategoryDetailPage), parameter);
    }
}
