using System.Windows.Input;
using VjWms.Desktop.UI.ViewModels.Issues;
using VjWms.Desktop.UI.Views;

namespace VjWms.Desktop.UI.Views.Pages;

public partial class IssueListView : System.Windows.Controls.UserControl
{
    public IssueListView() => InitializeComponent();

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataGridRowDoubleClick.TryGetItem<IssueRow>(e, out var row) && DataContext is IssueListViewModel vm
            && vm.ViewDetailCommand.CanExecute(row))
        {
            vm.ViewDetailCommand.Execute(row);
        }
    }
}
