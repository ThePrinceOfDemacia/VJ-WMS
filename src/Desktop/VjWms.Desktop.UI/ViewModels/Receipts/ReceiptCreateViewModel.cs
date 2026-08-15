using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Receipts;

public partial class ReceiptCreateViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;
    private readonly string _currentUser;

    // Header fields
    [ObservableProperty] private CachedWarehouse? _selectedWarehouse;
    [ObservableProperty] private CachedSupplier? _selectedSupplier;
    [ObservableProperty] private DateTime _receiptDate = DateTime.Today;
    [ObservableProperty] private string _contractNumber = "";
    [ObservableProperty] private string _roNumber = "";
    [ObservableProperty] private string _driverName = "";
    [ObservableProperty] private string _driverIdCard = "";
    [ObservableProperty] private string _vehicleNumber = "";
    [ObservableProperty] private string _trailerNumber = "";
    [ObservableProperty] private string _notes = "";
    [ObservableProperty] private string _errorMessage = "";

    // Dropdown sources
    public ObservableCollection<CachedWarehouse> Warehouses { get; } = new();
    public ObservableCollection<CachedSupplier> Suppliers { get; } = new();
    public ObservableCollection<CachedProduct> Products { get; } = new();

    // Line items
    public ObservableCollection<ReceiptLineItem> LineItems { get; } = new();

    public ReceiptCreateViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        _currentUser = "admin"; // TODO: inject current user
        Title = "Tạo Phiếu Nhập Kho";

        LoadMasterData();
        AddEmptyLine();
    }

    private void LoadMasterData()
    {
        foreach (var w in _db.CachedWarehouses.Where(w => w.IsActive).ToList())
            Warehouses.Add(w);
        foreach (var s in _db.CachedSuppliers.ToList())
            Suppliers.Add(s);
        foreach (var p in _db.CachedProducts.Where(p => p.IsActive).ToList())
            Products.Add(p);
    }

    [RelayCommand]
    private void AddLine() => AddEmptyLine();

    private void AddEmptyLine()
    {
        LineItems.Add(new ReceiptLineItem
        {
            LineNumber = LineItems.Count + 1,
            AvailableProducts = Products
        });
    }

    [RelayCommand]
    private void RemoveLine(ReceiptLineItem? line)
    {
        if (line != null && LineItems.Count > 1)
        {
            LineItems.Remove(line);
            // Renumber
            for (int i = 0; i < LineItems.Count; i++)
                LineItems[i].LineNumber = i + 1;
        }
    }

    [RelayCommand]
    private void SaveDraft()
    {
        if (!Validate()) return;
        SaveReceipt("Draft");
        _nav.NavigateTo<ReceiptListViewModel>();
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!Validate()) return;
        SaveReceipt("PendingSync");
        _nav.NavigateTo<ReceiptListViewModel>();
    }

    [RelayCommand]
    private void Cancel() => _nav.NavigateTo<ReceiptListViewModel>();

    private bool Validate()
    {
        if (SelectedWarehouse == null)
        {
            ErrorMessage = "Vui lòng chọn kho nhập!";
            return false;
        }

        var validLines = LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0).ToList();
        if (validLines.Count == 0)
        {
            ErrorMessage = "Cần ít nhất 1 dòng hàng hóa có sản phẩm và số lượng!";
            return false;
        }

        ErrorMessage = "";
        return true;
    }

    private void SaveReceipt(string status)
    {
        var now = DateTime.UtcNow.ToString("o");
        var date = ReceiptDate.ToString("yyyy/MM/dd");

        // Generate local number
        var todayCount = _db.LocalStockReceipts
            .Count(r => r.ReceiptDate == ReceiptDate.ToString("yyyy-MM-dd")) + 1;
        var localNumber = $"LOCAL/NK/{date}/{todayCount:D3}";

        var receipt = new LocalStockReceipt
        {
            LocalNumber = localNumber,
            WarehouseId = SelectedWarehouse!.Id,
            SupplierId = SelectedSupplier?.Id,
            ReceiptDate = ReceiptDate.ToString("yyyy-MM-dd"),
            ContractNumber = string.IsNullOrWhiteSpace(ContractNumber) ? null : ContractNumber,
            RoNumber = string.IsNullOrWhiteSpace(RoNumber) ? null : RoNumber,
            DriverName = string.IsNullOrWhiteSpace(DriverName) ? null : DriverName,
            DriverIdCard = string.IsNullOrWhiteSpace(DriverIdCard) ? null : DriverIdCard,
            VehicleNumber = string.IsNullOrWhiteSpace(VehicleNumber) ? null : VehicleNumber,
            TrailerNumber = string.IsNullOrWhiteSpace(TrailerNumber) ? null : TrailerNumber,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
            Status = status,
            SyncStatus = status == "PendingSync" ? "Pending" : "Pending",
            CreatedBy = _currentUser,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var line in LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0))
        {
            receipt.Items.Add(new LocalStockReceiptItem
            {
                ReceiptId = receipt.Id,
                ProductId = line.SelectedProduct!.Id,
                Quantity = line.Quantity,
                Unit = line.SelectedProduct.Unit,
                UnitPrice = line.UnitPrice > 0 ? line.UnitPrice : null,
                LotNumber = string.IsNullOrWhiteSpace(line.LotNumber) ? null : line.LotNumber,
                BatchNumber = string.IsNullOrWhiteSpace(line.BatchNumber) ? null : line.BatchNumber,
                NumberOfBags = line.NumberOfBags > 0 ? line.NumberOfBags : null,
                PackagingType = line.PackagingType,
                Notes = string.IsNullOrWhiteSpace(line.LineNotes) ? null : line.LineNotes
            });
        }

        _db.LocalStockReceipts.Add(receipt);
        _db.SaveChanges();
    }
}

public partial class ReceiptLineItem : ObservableObject
{
    [ObservableProperty] private int _lineNumber;
    [ObservableProperty] private CachedProduct? _selectedProduct;
    [ObservableProperty] private double _quantity;
    [ObservableProperty] private double _unitPrice;
    [ObservableProperty] private string _lotNumber = "";
    [ObservableProperty] private string _batchNumber = "";
    [ObservableProperty] private int _numberOfBags;
    [ObservableProperty] private string _packagingType = "Pallet";
    [ObservableProperty] private string _lineNotes = "";

    public ObservableCollection<CachedProduct> AvailableProducts { get; set; } = new();

    partial void OnSelectedProductChanged(CachedProduct? value)
    {
        // Auto-fill unit when product is selected
        OnPropertyChanged(nameof(UnitDisplay));
    }

    public string UnitDisplay => SelectedProduct?.Unit ?? "KG";
}
