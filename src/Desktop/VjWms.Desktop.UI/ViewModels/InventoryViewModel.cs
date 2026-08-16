using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

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

        var warehouses = _db.CachedWarehouses.ToDictionary(w => w.Id);
        var products = _db.CachedProducts.ToDictionary(p => p.Id);

        var snapshotDate = _db.CachedInventories.ToList()
            .ToDictionary(i => (i.WarehouseId, i.ProductId), i => i.SnapshotAt);

        var all = InventoryCalculator.CalculateAll(_db);
        var rows = new List<InventoryRow>();

        foreach (var (key, qty) in all)
        {
            if (SelectedWarehouse != null && !string.IsNullOrEmpty(SelectedWarehouse.Id)
                && key.WarehouseId != SelectedWarehouse.Id)
                continue;

            if (!products.TryGetValue(key.ProductId, out var product)) continue;

            if (!string.IsNullOrWhiteSpace(SearchText)
                && !product.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                && !product.ProductCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                continue;

            var warehouseName = warehouses.TryGetValue(key.WarehouseId, out var wh) ? wh.Name : key.WarehouseId;
            var warehouseCode = wh?.Code ?? "";

            rows.Add(new InventoryRow
            {
                WarehouseCode = warehouseCode,
                WarehouseName = warehouseName,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Quantity = qty.Actual,
                ExpectedQuantity = qty.Expected,
                Unit = product.Unit,
                SnapshotDate = snapshotDate.TryGetValue(key, out var sd) ? sd : ""
            });
        }

        foreach (var row in rows.OrderBy(r => r.WarehouseCode).ThenBy(r => r.ProductCode))
            InventoryItems.Add(row);
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
    public double ExpectedQuantity { get; set; }
    public string Unit { get; set; } = "";
    public string SnapshotDate { get; set; } = "";
}
