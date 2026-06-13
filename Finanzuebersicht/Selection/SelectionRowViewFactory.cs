using Finanzuebersicht.Converters;

namespace Finanzuebersicht.Selection;

internal static class SelectionRowViewFactory
{
    private const double RowHeight = 46;

    public static View CreateRow(SelectionRow row, Func<SelectionRow, Task> onSelectedAsync)
    {
        var backgroundColor = row.IsSelected
            ? ColorResourceHelper.GetThemeColor(
                "Gray100", "Gray900",
                Color.FromArgb("#F2F2F7"), Color.FromArgb("#2C2C2E"))
            : Colors.Transparent;

        var label = new Label
        {
            Text = row.DisplayText,
            FontSize = 16,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            VerticalOptions = LayoutOptions.Center,
            TextColor = ColorResourceHelper.GetThemeColor(
                "TextPrimary", "TextPrimaryDark",
                Color.FromArgb("#000000"), Color.FromArgb("#FFFFFF"))
        };

        var check = new Label
        {
            Text = "✓",
            FontSize = 15,
            IsVisible = row.IsSelected,
            VerticalOptions = LayoutOptions.Center,
            TextColor = ColorResourceHelper.GetThemeColor(
                "Primary", "PrimaryDark",
                Color.FromArgb("#007AFF"), Color.FromArgb("#0A84FF"))
        };

        var separator = new BoxView
        {
            HeightRequest = 0.5,
            Color = ColorResourceHelper.GetThemeColor(
                "Separator", "SeparatorDark",
                Color.FromArgb("#E5E5EA"), Color.FromArgb("#38383A")),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill
        };

        var rowGrid = new Grid
        {
            HeightRequest = RowHeight,
            Padding = new Thickness(16, 0, 12, 0),
            BackgroundColor = backgroundColor,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Children = { label, check, separator }
        };
        Grid.SetColumn(label, 0);
        Grid.SetColumn(check, 1);
        Grid.SetRowSpan(separator, 1);

        var button = new Button
        {
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        button.Clicked += async (_, _) => await onSelectedAsync(row);

        return new Grid
        {
            HeightRequest = RowHeight,
            Children = { rowGrid, button }
        };
    }

    public static double RowHeightRequest => RowHeight;
}
