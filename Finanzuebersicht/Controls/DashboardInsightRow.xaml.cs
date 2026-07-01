using System.Windows.Input;

namespace Finanzuebersicht.Controls;

public partial class DashboardInsightRow : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DashboardInsightRow), string.Empty);

    public static readonly BindableProperty DetailProperty =
        BindableProperty.Create(nameof(Detail), typeof(string), typeof(DashboardInsightRow), string.Empty);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(DashboardInsightRow),
            propertyChanged: OnTapCommandChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    public bool IsInteractive => TapCommand?.CanExecute(null) == true;

    public DashboardInsightRow()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Detail))
            OnPropertyChanged(nameof(HasDetail));
    }

    private static void OnTapCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var row = (DashboardInsightRow)bindable;
        row.OnPropertyChanged(nameof(IsInteractive));

        if (oldValue is ICommand oldCommand)
            oldCommand.CanExecuteChanged -= row.OnTapCommandCanExecuteChanged;
        if (newValue is ICommand newCommand)
            newCommand.CanExecuteChanged += row.OnTapCommandCanExecuteChanged;
    }

    private void OnTapCommandCanExecuteChanged(object? sender, EventArgs e)
        => OnPropertyChanged(nameof(IsInteractive));
}
