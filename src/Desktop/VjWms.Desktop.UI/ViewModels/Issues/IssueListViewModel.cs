using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Issues;

public partial class IssueListViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;

    [ObservableProperty] private string _filterStatus = "All";

    public ObservableCollection<IssueRow> Issues { get; } = new();

    public IssueListViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        Title = "Phiếu Xuất Kho";
        LoadIssues();
    }

    private void LoadIssues()
    {
        Issues.Clear();
        var query = _db.LocalStockIssues
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .AsQueryable();

        if (FilterStatus != "All")
            query = query.Where(i => i.Status == FilterStatus);

        foreach (var i in query.ToList())
        {
            var warehouse = _db.CachedWarehouses.Find(i.WarehouseId);
            var customer = i.CustomerId != null ? _db.CachedCustomers.Find(i.CustomerId) : null;

            Issues.Add(new IssueRow
            {
                Id = i.Id,
                LocalNumber = i.LocalNumber,
                OfficialNumber = i.OfficialNumber,
                WarehouseName = warehouse?.Name ?? i.WarehouseId,
                CustomerName = customer?.Name ?? "",
                IssueDate = i.IssueDate,
                ItemCount = i.Items.Count,
                Status = i.Status,
                SyncStatus = i.SyncStatus,
                IsReadOnly = i.IsReadOnly
            });
        }
    }

    [RelayCommand]
    private void CreateNew() => _nav.NavigateTo<IssueCreateViewModel>();

    [RelayCommand]
    private void Refresh() => LoadIssues();

    [RelayCommand]
    private void Delete(IssueRow? row)
    {
        if (row == null || row.IsReadOnly || row.Status != "Draft") return;

        var issue = _db.LocalStockIssues.Include(i => i.Items).FirstOrDefault(i => i.Id == row.Id);
        if (issue != null)
        {
            _db.LocalStockIssueItems.RemoveRange(issue.Items);
            _db.LocalStockIssues.Remove(issue);
            _db.SaveChanges();
            LoadIssues();
        }
    }

    partial void OnFilterStatusChanged(string value) => LoadIssues();
}

public class IssueRow
{
    public string Id { get; set; } = "";
    public string LocalNumber { get; set; } = "";
    public string? OfficialNumber { get; set; }
    public string WarehouseName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string IssueDate { get; set; } = "";
    public int ItemCount { get; set; }
    public string Status { get; set; } = "";
    public string SyncStatus { get; set; } = "";
    public bool IsReadOnly { get; set; }
}
