namespace Finanzuebersicht.Selection;

public sealed class SelectionRow
{
    public SelectionRow(object item, string displayText, bool isSelected)
    {
        Item = item;
        DisplayText = displayText;
        IsSelected = isSelected;
    }

    public object Item { get; }

    public string DisplayText { get; }

    public bool IsSelected { get; }
}
