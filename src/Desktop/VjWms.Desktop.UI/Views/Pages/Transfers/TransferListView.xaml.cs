using System.Windows.Input;
using VjWms.Desktop.UI.ViewModels.Transfers;
using VjWms.Desktop.UI.Views;

namespace VjWms.Desktop.UI.Views.Pages.Transfers;

public partial class TransferListView
{
    public TransferListView() => InitializeComponent();

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataGridRowDoubleClick.TryGetItem<TransferRow>(e, out var row) && DataContext is TransferListViewModel vm
            && vm.ViewDetailCommand.CanExecute(row))
        {
            vm.ViewDetailCommand.Execute(row);
        }
    }
}
