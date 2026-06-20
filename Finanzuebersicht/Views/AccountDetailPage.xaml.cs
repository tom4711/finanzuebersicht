using Finanzuebersicht.Navigation;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class AccountDetailPage : ContentPage, IQueryAttributable
{
    public AccountDetailPage(AccountDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        App.LanguageChanged += OnLocalizationChanged;
        App.CurrencyChanged += OnLocalizationChanged;
    }

    protected override void OnDisappearing()
    {
        App.LanguageChanged -= OnLocalizationChanged;
        App.CurrencyChanged -= OnLocalizationChanged;
        base.OnDisappearing();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is IApplyQueryAttributes vm)
            vm.ApplyQueryAttributes(query);
    }

    private void OnLocalizationChanged()
    {
        if (BindingContext is ILocalizableViewModel vm)
            vm.RefreshLocalizedStrings();
    }
}
