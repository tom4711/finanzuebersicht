using System.Collections;
using CommunityToolkit.Maui.Extensions;
using Finanzuebersicht.Converters;
using Finanzuebersicht.Selection;
using Finanzuebersicht.Views.Popups;

namespace Finanzuebersicht.Controls;

public partial class SelectionField : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SelectionField));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SelectionField), null, BindingMode.TwoWay, propertyChanged: OnSelectionChanged);

    public static readonly BindableProperty DisplayMemberPathProperty =
        BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(SelectionField), "Name");

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SelectionField), string.Empty);

    public static readonly BindableProperty ResolvedDisplayTextProperty =
        BindableProperty.Create(nameof(ResolvedDisplayText), typeof(string), typeof(SelectionField), string.Empty);

    public static readonly BindableProperty ResolvedTextColorProperty =
        BindableProperty.Create(nameof(ResolvedTextColor), typeof(Color), typeof(SelectionField), Colors.Gray);

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string ResolvedDisplayText
    {
        get => (string)GetValue(ResolvedDisplayTextProperty);
        set => SetValue(ResolvedDisplayTextProperty, value);
    }

    public Color ResolvedTextColor
    {
        get => (Color)GetValue(ResolvedTextColorProperty);
        set => SetValue(ResolvedTextColorProperty, value);
    }

    public SelectionField()
    {
        InitializeComponent();
        UpdateResolvedDisplay();
    }

    private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SelectionField field)
            field.UpdateResolvedDisplay();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName is nameof(SelectedItem) or nameof(Placeholder) or nameof(DisplayMemberPath))
            UpdateResolvedDisplay();
    }

    private void UpdateResolvedDisplay()
    {
        if (SelectedItem is not null)
        {
            ResolvedDisplayText = SelectionDisplayHelper.GetDisplayText(SelectedItem, DisplayMemberPath);
            ResolvedTextColor = ColorResourceHelper.GetThemeColor(
                "TextPrimary", "TextPrimaryDark",
                Color.FromArgb("#000000"), Color.FromArgb("#FFFFFF"));
            return;
        }

        ResolvedDisplayText = string.IsNullOrWhiteSpace(Placeholder) ? "—" : Placeholder;
        ResolvedTextColor = ColorResourceHelper.GetThemeColor(
            "Gray500", "Gray600",
            Color.FromArgb("#AEAEB2"), Color.FromArgb("#8E8E93"));
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (ItemsSource is null)
            return;

        var popup = new SelectionPopup(ItemsSource, SelectedItem, DisplayMemberPath);
        var page = GetParentPage();
        if (page is null)
            return;

        var popupResult = await page.ShowPopupAsync<object>(popup);
        if (popupResult.WasDismissedByTappingOutsideOfPopup || popupResult.Result is null)
            return;

        SelectedItem = popupResult.Result;
        UpdateResolvedDisplay();
    }

    private Page? GetParentPage()
    {
        Element? parent = this;
        while (parent is not null)
        {
            if (parent is Page page)
                return page;
            parent = parent.Parent;
        }

        return Shell.Current?.CurrentPage;
    }
}
