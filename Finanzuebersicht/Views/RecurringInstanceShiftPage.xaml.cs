using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class RecurringInstanceShiftPage : ContentPage
{
    public RecurringInstanceShiftPage(RecurringInstanceShiftViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
