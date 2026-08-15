using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Receipts;

public partial class ReceiptListViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;

    [ObservableProperty] private string _filterStatus = "All";

    public ObservableCollection<ReceiptRow> Receipts { get; } = new();

    public ReceiptListViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        Title = "Phiếu Nhập Kho";
        LoadReceipts();
    }

    private void LoadReceipts()
    {
        Receipts.Clear();
        var query = _db.LocalStockReceipts
            .Include(r => r.Items)
            .OrderByDescending(r => r.CreatedAt)
            .AsQueryable();

        if (FilterStatus != "All")
            query = query.Where(r => r.Status == FilterStatus);

        foreach (var r in query.ToList())
        {
            var warehouse = _db.CachedWarehouses.Find(r.WarehouseId);
            var supplier = r.SupplierId != null ? _db.CachedSuppliers.Find(r.SupplierId) : null;

            Receipts.Add(new ReceiptRow
            {
                Id = r.Id,
                LocalNumber = r.LocalNumber,
                OfficialNumber = r.OfficialNumber,
                WarehouseName = warehouse?.Name ?? r.WarehouseId,
                SupplierName = supplier?.Name ?? "",
                ReceiptDate = r.ReceiptDate,
                ItemCount = r.Items.Count,
                Status = r.Status,
                SyncStatus = r.SyncStatus,
                IsReadOnly = r.IsReadOnly
            });
        }
    }

    [RelayCommand]
    private void CreateNew() => _nav.NavigateTo<ReceiptCreateViewModel>();

    [RelayCommand]
    private void Refresh() => LoadReceipts();

    [RelayCommand]
    private void Delete(ReceiptRow? row)
    {
        if (row == null || row.IsReadOnly || row.Status != "Draft") return;

        var receipt = _db.LocalStockReceipts.Include(r => r.Items).FirstOrDefault(r => r.Id == row.Id);
        if (receipt != null)
        {
            _db.LocalStockReceiptItems.RemoveRange(receipt.Items);
            _db.LocalStockReceipts.Remove(receipt);
            _db.SaveChanges();
            LoadReceipts();
        }
    }

    [RelayCommand]
    private void Edit(ReceiptRow? row)
    {
        if (row == null || row.IsReadOnly) return;
        // Navigate to create view with existing receipt ID for editing
        // For Phase 1, we'll just navigate to create (edit comes later)
        _nav.NavigateTo<ReceiptCreateViewModel>();
    }

    partial void OnFilterStatusChanged(string value) => LoadReceipts();
}

public class ReceiptRow
{
    public string Id { get; set; } = "";
    public string LocalNumber { get; set; } = "";
    public string? OfficialNumber { get; set; }
    public string WarehouseName { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public string ReceiptDate { get; set; } = "";
    public int ItemCount { get; set; }
    public string Status { get; set; } = "";
    public string SyncStatus { get; set; } = "";
    public bool IsReadOnly { get; set; }
}
