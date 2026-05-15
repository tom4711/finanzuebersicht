using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class CategoriesPage : BaseContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
