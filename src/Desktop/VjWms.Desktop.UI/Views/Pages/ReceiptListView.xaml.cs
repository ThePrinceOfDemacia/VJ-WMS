using System.Windows.Input;
using VjWms.Desktop.UI.ViewModels.Receipts;
using VjWms.Desktop.UI.Views;

namespace VjWms.Desktop.UI.Views.Pages;

public partial class ReceiptListView : System.Windows.Controls.UserControl
{
    public ReceiptListView() => InitializeComponent();

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataGridRowDoubleClick.TryGetItem<ReceiptRow>(e, out var row) && DataContext is ReceiptListViewModel vm
            && vm.ViewDetailCommand.CanExecute(row))
        {
            vm.ViewDetailCommand.Execute(row);
        }
    }
}
