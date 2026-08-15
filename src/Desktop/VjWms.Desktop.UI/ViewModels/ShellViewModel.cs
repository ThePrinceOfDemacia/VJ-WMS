using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.UI.Services;
using VjWms.Desktop.UI.ViewModels.Issues;
using VjWms.Desktop.UI.ViewModels.Receipts;

namespace VjWms.Desktop.UI.ViewModels;

public partial class ShellViewModel : BaseViewModel
{
    private readonly INavigationService _nav;

    [ObservableProperty] private BaseViewModel? _currentPage;
    [ObservableProperty] private string _currentPageTitle = "Dashboard";
    [ObservableProperty] private string _username = "admin";
    [ObservableProperty] private bool _isOnline;

    public ShellViewModel(INavigationService nav)
    {
        _nav = nav;
        _nav.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Start on Dashboard
        _nav.NavigateTo<DashboardViewModel>();
    }

    private void OnCurrentViewModelChanged()
    {
        CurrentPage = _nav.CurrentViewModel;
        CurrentPageTitle = CurrentPage?.Title ?? "Dashboard";
    }

    [RelayCommand]
    private void NavigateToDashboard() => _nav.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private void NavigateToReceipts() => _nav.NavigateTo<ReceiptListViewModel>();

    [RelayCommand]
    private void NavigateToIssues() => _nav.NavigateTo<IssueListViewModel>();

    [RelayCommand]
    private void NavigateToInventory() => _nav.NavigateTo<InventoryViewModel>();

    [RelayCommand]
    private void NavigateToWarehouses() => _nav.NavigateTo<MasterData.WarehouseListViewModel>();

    [RelayCommand]
    private void NavigateToProducts() => _nav.NavigateTo<MasterData.ProductListViewModel>();
}
