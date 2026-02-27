using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class CategoryDetailPage : ContentPage
{
    public CategoryDetailPage(CategoryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
