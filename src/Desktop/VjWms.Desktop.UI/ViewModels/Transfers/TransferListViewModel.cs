using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Transfers;

public partial class TransferListViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;

    [ObservableProperty] private string _filterStatus = "All";

    public ObservableCollection<TransferRow> Transfers { get; } = new();

    public TransferListViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        Title = "Luân Chuyển Kho";
        LoadTransfers();
    }

    private void LoadTransfers()
    {
        Transfers.Clear();
        var query = _db.LocalTransfers
            .Include(t => t.Items)
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();

        if (FilterStatus != "All")
            query = query.Where(t => t.Status == FilterStatus);

        var warehouses = _db.CachedWarehouses.ToDictionary(w => w.Id);

        foreach (var t in query.ToList())
        {
            var srcName = warehouses.TryGetValue(t.SourceWarehouseId, out var src) ? src.Name : t.SourceWarehouseId;
            var dstName = warehouses.TryGetValue(t.DestWarehouseId, out var dst) ? dst.Name : t.DestWarehouseId;

            Transfers.Add(new TransferRow
            {
                Id = t.Id,
                LocalNumber = t.LocalNumber,
                OfficialNumber = t.OfficialNumber,
                SourceWarehouseName = srcName,
                DestWarehouseName = dstName,
                TransferType = t.TransferType,
                TransferDate = t.TransferDate,
                ItemCount = t.Items.Count,
                Status = t.Status,
                SyncStatus = t.SyncStatus,
                IsReadOnly = t.IsReadOnly
            });
        }
    }

    [RelayCommand]
    private void CreateNew() => _nav.NavigateTo<TransferCreateViewModel>();

    [RelayCommand]
    private void Refresh() => LoadTransfers();

    [RelayCommand]
    private void Delete(TransferRow? row)
    {
        if (row == null || !row.CanEdit) return;

        var transfer = _db.LocalTransfers.Include(t => t.Items).FirstOrDefault(t => t.Id == row.Id);
        if (transfer != null)
        {
            _db.LocalTransferItems.RemoveRange(transfer.Items.ToList());
            _db.LocalTransfers.Remove(transfer);
            _db.SaveChanges();
            LoadTransfers();
        }
    }

    [RelayCommand]
    private void Edit(TransferRow? row)
    {
        if (row == null || !row.CanEdit) return;
        _nav.NavigateTo<TransferCreateViewModel>(row.Id);
    }

    [RelayCommand]
    private void Sync(TransferRow? row)
    {
        if (row == null || !row.CanSync) return;

        var transfer = _db.LocalTransfers.FirstOrDefault(t => t.Id == row.Id);
        if (transfer != null)
        {
            transfer.Status = "SyncFailed"; // Simulate failed sync since no API yet
            transfer.SyncStatus = "Failed";
            _db.SaveChanges();
            LoadTransfers();
        }
    }

    [RelayCommand]
    private void ViewDetail(TransferRow? row)
    {
        if (row == null) return;
        _nav.NavigateTo<TransactionDetailViewModel>(new TransactionDetailParams
        {
            DocumentId = row.Id,
            DocumentType = "Transfer"
        });
    }

    partial void OnFilterStatusChanged(string value) => LoadTransfers();
}

public class TransferRow
{
    public string Id { get; set; } = "";
    public string LocalNumber { get; set; } = "";
    public string? OfficialNumber { get; set; }
    public string SourceWarehouseName { get; set; } = "";
    public string DestWarehouseName { get; set; } = "";
    public string TransferType { get; set; } = "";
    public string TransferDate { get; set; } = "";
    public int ItemCount { get; set; }
    public string Status { get; set; } = "";
    public string SyncStatus { get; set; } = "";
    public bool IsReadOnly { get; set; }

    public bool CanEdit => !IsReadOnly && (Status == "Draft" || Status == "PendingSync" || Status == "SyncFailed");
    public bool CanSync => !IsReadOnly && (Status == "Draft" || Status == "PendingSync" || Status == "SyncFailed");
}
