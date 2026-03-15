using Microsoft.Extensions.Logging;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Views;

public partial class YearOverviewPage : ContentPage
{
    private readonly YearOverviewViewModel _vm;
    private readonly ILogger<YearOverviewPage>? _logger;

    public YearOverviewPage(YearOverviewViewModel vm, ILogger<YearOverviewPage>? logger = null)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        _logger = logger;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "YearOverviewPage error");
        }
    }
}
