using System.ComponentModel;
using System.Windows.Input;

namespace Finanzuebersicht.Controls;

public partial class EmptyStateView : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty HintProperty =
        BindableProperty.Create(nameof(Hint), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(EmptyStateView), string.Empty,
            propertyChanged: OnIconPropertyChanged);

    public static readonly BindableProperty IconImageProperty =
        BindableProperty.Create(nameof(IconImage), typeof(string), typeof(EmptyStateView), string.Empty,
            propertyChanged: OnIconPropertyChanged);

    public static readonly BindableProperty IconAccessibilityDescriptionProperty =
        BindableProperty.Create(nameof(IconAccessibilityDescription), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(EmptyStateView));

    public EmptyStateView() => InitializeComponent();

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public string IconImage
    {
        get => (string)GetValue(IconImageProperty);
        set => SetValue(IconImageProperty, value);
    }

    public string IconAccessibilityDescription
    {
        get => (string)GetValue(IconAccessibilityDescriptionProperty);
        set => SetValue(IconAccessibilityDescriptionProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public bool HasHint => !string.IsNullOrWhiteSpace(Hint);
    public bool HasIconImage => !string.IsNullOrWhiteSpace(IconImage);
    public bool ShowIconText => !HasIconImage && !string.IsNullOrWhiteSpace(IconText);
    public bool HasAction => !string.IsNullOrWhiteSpace(ActionText) && ActionCommand != null;

    private static void OnIconPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyStateView view)
        {
            view.OnPropertyChanged(nameof(HasIconImage));
            view.OnPropertyChanged(nameof(ShowIconText));
        }
    }
}
