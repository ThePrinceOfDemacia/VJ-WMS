using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI.ViewModels;

public partial class InventoryViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;

    [ObservableProperty] private CachedWarehouse? _selectedWarehouse;
    [ObservableProperty] private string _searchText = "";

    public ObservableCollection<CachedWarehouse> Warehouses { get; } = new();
    public ObservableCollection<InventoryRow> InventoryItems { get; } = new();

    public InventoryViewModel(LocalDbContext db)
    {
        _db = db;
        Title = "Tra cứu tồn kho";

        // Add "All" option
        Warehouses.Add(new CachedWarehouse { Id = "", Code = "ALL", Name = "-- Tất cả kho --" });
        foreach (var w in _db.CachedWarehouses.Where(w => w.IsActive).ToList())
            Warehouses.Add(w);

        LoadInventory();
    }

    private void LoadInventory()
    {
        InventoryItems.Clear();

        var inventory = _db.CachedInventories.ToList();
        var warehouses = _db.CachedWarehouses.ToDictionary(w => w.Id);
        var products = _db.CachedProducts.ToDictionary(p => p.Id);

        // Calculate adjustments from receipts and issues
        var receiptAdjustments = _db.LocalStockReceiptItems
            .Where(ri => ri.Receipt.Status != "Draft") // Only count confirmed receipts
            .GroupBy(ri => new { ri.Receipt.WarehouseId, ri.ProductId })
            .Select(g => new { g.Key.WarehouseId, g.Key.ProductId, Qty = g.Sum(x => x.Quantity) })
            .ToList();

        var issueAdjustments = _db.LocalStockIssueItems
            .Where(ii => ii.Issue.Status != "Draft") // Only count confirmed issues
            .GroupBy(ii => new { ii.Issue.WarehouseId, ii.ProductId })
            .Select(g => new { g.Key.WarehouseId, g.Key.ProductId, Qty = g.Sum(x => x.Quantity) })
            .ToList();

        foreach (var inv in inventory)
        {
            if (SelectedWarehouse != null && !string.IsNullOrEmpty(SelectedWarehouse.Id)
                && inv.WarehouseId != SelectedWarehouse.Id)
                continue;

            if (!products.TryGetValue(inv.ProductId, out var product)) continue;

            if (!string.IsNullOrWhiteSpace(SearchText)
                && !product.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                && !product.ProductCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                continue;

            var warehouseName = warehouses.TryGetValue(inv.WarehouseId, out var wh) ? wh.Name : inv.WarehouseId;
            var warehouseCode = wh?.Code ?? "";

            // Add receipt adjustments
            var receiptQty = receiptAdjustments
                .Where(r => r.WarehouseId == inv.WarehouseId && r.ProductId == inv.ProductId)
                .Sum(r => r.Qty);

            // Subtract issue adjustments
            var issueQty = issueAdjustments
                .Where(i => i.WarehouseId == inv.WarehouseId && i.ProductId == inv.ProductId)
                .Sum(i => i.Qty);

            var currentQty = inv.Quantity + receiptQty - issueQty;

            InventoryItems.Add(new InventoryRow
            {
                WarehouseCode = warehouseCode,
                WarehouseName = warehouseName,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Quantity = currentQty,
                Unit = product.Unit,
                SnapshotDate = inv.SnapshotAt
            });
        }
    }

    [RelayCommand]
    private void Refresh() => LoadInventory();

    partial void OnSelectedWarehouseChanged(CachedWarehouse? value) => LoadInventory();
    partial void OnSearchTextChanged(string value) => LoadInventory();
}

public class InventoryRow
{
    public string WarehouseCode { get; set; } = "";
    public string WarehouseName { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public double Quantity { get; set; }
    public string Unit { get; set; } = "";
    public string SnapshotDate { get; set; } = "";
}
