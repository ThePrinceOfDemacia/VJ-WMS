using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;

    [ObservableProperty] private int _activeWarehouses;
    [ObservableProperty] private int _draftCount;
    [ObservableProperty] private int _pendingSyncCount;
    [ObservableProperty] private int _rejectedCount;
    [ObservableProperty] private int _totalProducts;
    [ObservableProperty] private int _totalReceipts;
    [ObservableProperty] private int _totalIssues;

    public ObservableCollection<RecentActivity> RecentActivities { get; } = new();

    public DashboardViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        Title = "Dashboard";
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            ActiveWarehouses = _db.CachedWarehouses.Count(w => w.IsActive);
            TotalProducts = _db.CachedProducts.Count(p => p.IsActive);

            // Receipts stats
            var receipts = _db.LocalStockReceipts.ToList();
            var issues = _db.LocalStockIssues.ToList();

            TotalReceipts = receipts.Count;
            TotalIssues = issues.Count;

            DraftCount = receipts.Count(r => r.Status == "Draft")
                       + issues.Count(i => i.Status == "Draft");

            PendingSyncCount = receipts.Count(r => r.SyncStatus == "Pending" && r.Status == "PendingSync")
                             + issues.Count(i => i.SyncStatus == "Pending" && i.Status == "PendingSync");

            RejectedCount = receipts.Count(r => r.SyncStatus == "Rejected")
                          + issues.Count(i => i.SyncStatus == "Rejected");

            // Recent activities
            RecentActivities.Clear();
            var recentReceipts = receipts
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentActivity
                {
                    Icon = "📥",
                    Description = $"Phiếu nhập {r.LocalNumber}",
                    Status = r.Status,
                    Date = r.CreatedAt
                });

            var recentIssues = issues
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new RecentActivity
                {
                    Icon = "📤",
                    Description = $"Phiếu xuất {i.LocalNumber}",
                    Status = i.Status,
                    Date = i.CreatedAt
                });

            foreach (var activity in recentReceipts.Concat(recentIssues)
                         .OrderByDescending(a => a.Date).Take(8))
            {
                RecentActivities.Add(activity);
            }
        }
        catch
        {
            // Silently handle if database is empty
        }
    }

    [RelayCommand]
    private void Refresh() => LoadData();

    [RelayCommand]
    private void NavigateToReceipts() => _nav.NavigateTo<Receipts.ReceiptListViewModel>();

    [RelayCommand]
    private void NavigateToIssues() => _nav.NavigateTo<Issues.IssueListViewModel>();
}

public class RecentActivity
{
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string Date { get; set; } = "";
}
