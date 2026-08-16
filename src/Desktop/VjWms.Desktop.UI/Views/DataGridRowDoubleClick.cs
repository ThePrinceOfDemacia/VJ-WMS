using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VjWms.Desktop.UI.Views;

/// <summary>
/// Reliable "double-click a DataGrid row" detection. Control.MouseDoubleClick is unreliable on
/// DataGrid in practice - a cell/row can mark the underlying mouse event Handled during its own
/// selection or edit-entry processing before the bubble phase ever reaches the DataGrid, so
/// MouseDoubleClick sometimes never fires at all. PreviewMouseLeftButtonDown tunnels top-down
/// and always sees the raw click before any descendant gets a chance to swallow it.
/// </summary>
public static class DataGridRowDoubleClick
{
    /// <summary>
    /// Returns true and the row's bound item if this event is a double-click that landed on an
    /// actual data row (not the header, empty space, or an action button within the row - those
    /// should keep handling their own click instead of also opening the detail view).
    /// </summary>
    public static bool TryGetItem<T>(MouseButtonEventArgs e, out T? item) where T : class
    {
        item = null;
        if (e.ClickCount != 2) return false;

        var node = e.OriginalSource as DependencyObject;
        while (node != null)
        {
            if (node is Button) return false;
            if (node is DataGridRow { Item: T match })
            {
                item = match;
                return true;
            }
            node = VisualTreeHelper.GetParent(node);
        }

        return false;
    }
}
