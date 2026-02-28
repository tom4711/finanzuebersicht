using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class YearOverviewPage : ContentPage
{
    public YearOverviewPage(YearOverviewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
