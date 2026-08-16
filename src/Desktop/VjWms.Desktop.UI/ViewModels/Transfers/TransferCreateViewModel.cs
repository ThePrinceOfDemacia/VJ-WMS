using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;
using VjWms.Desktop.UI.Services.Scanner;
using VjWms.Desktop.UI.Validation;
using VjWms.Desktop.UI.ViewModels.Attachments;

namespace VjWms.Desktop.UI.ViewModels.Transfers;

public partial class TransferCreateViewModel : BaseViewModel, IParameterReceiver
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;
    private readonly IScannerService _scanner;
    private readonly string _currentUser;
    private string _transferId;
    private bool _isEditMode;

    // Header fields
    [ObservableProperty] private CachedWarehouse? _selectedSourceWarehouse;
    [ObservableProperty] private CachedWarehouse? _selectedDestWarehouse;
    [ObservableProperty] private string _transferType = "Physical";
    [ObservableProperty] private DateTime _transferDate = DateTime.Today;
    [ObservableProperty] private string _notes = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isTransferTypeLocked;

    // Dropdown sources
    public ObservableCollection<CachedWarehouse> Warehouses { get; } = new();
    public ObservableCollection<CachedProduct> Products { get; } = new();
    public ObservableCollection<string> TransferTypes { get; } = new() { "Logical", "Physical" };

    // Line items
    public ObservableCollection<TransferLineItem> LineItems { get; } = new();

    // Attachments
    public AttachmentPanelViewModel AttachmentPanel { get; }

    // Scanner
    [ObservableProperty] private bool _isHidListening;
    [ObservableProperty] private string _scanStatusMessage = "";

    // Inventory lookup for the selected source warehouse
    private Dictionary<string, double> _warehouseInventory = new();

    public TransferCreateViewModel(LocalDbContext db, INavigationService nav, IScannerService scanner, AttachmentService attachmentService)
    {
        _db = db;
        _nav = nav;
        _scanner = scanner;
        _currentUser = "admin"; // TODO: inject current user
        _transferId = Guid.NewGuid().ToString();
        Title = "Tạo Phiếu Luân Chuyển Kho";

        AttachmentPanel = new AttachmentPanelViewModel(attachmentService, _transferId, "Transfer");

        LoadMasterData();
        AddEmptyLine();
    }

    public void ReceiveParameter(object parameter)
    {
        if (parameter is string transferId)
        {
            LoadExistingTransfer(transferId);
        }
    }

    private void LoadExistingTransfer(string transferId)
    {
        var transfer = _db.LocalTransfers
            .Include(t => t.Items)
            .FirstOrDefault(t => t.Id == transferId);

        if (transfer == null) return;

        _isEditMode = true;
        _transferId = transferId;
        Title = "Chỉnh sửa Phiếu Luân Chuyển Kho";

        SelectedSourceWarehouse = Warehouses.FirstOrDefault(w => w.Id == transfer.SourceWarehouseId);
        SelectedDestWarehouse = Warehouses.FirstOrDefault(w => w.Id == transfer.DestWarehouseId);
        TransferType = transfer.TransferType;
        TransferDate = DateTime.TryParse(transfer.TransferDate, out var dt) ? dt : DateTime.Today;
        Notes = transfer.Notes ?? "";

        LineItems.Clear();
        int lineNum = 1;
        foreach (var item in transfer.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            LineItems.Add(new TransferLineItem
            {
                LineNumber = lineNum++,
                SelectedProduct = product,
                Quantity = item.Quantity,
                LotNumber = item.LotNumber ?? "",
                BatchNumber = item.BatchNumber ?? "",
                AvailableProducts = Products
            });
        }

        if (LineItems.Count == 0) AddEmptyLine();
        
        UpdateAvailableProducts();
    }

    private void LoadMasterData()
    {
        foreach (var w in _db.CachedWarehouses.Where(w => w.IsActive).ToList())
            Warehouses.Add(w);
        foreach (var p in _db.CachedProducts.Where(p => p.IsActive).ToList())
            Products.Add(p);
    }

    /// <summary>
    /// Effective quantity available to move out of the given warehouse right now: the cached
    /// snapshot plus every locally pending Receipt/Issue/Transfer that touches it (see
    /// InventoryCalculator.Expected). When editing an existing transfer, that transfer's own
    /// previously-saved quantities are excluded so they don't count against themselves.
    /// </summary>
    private Dictionary<string, double> CalculateWarehouseInventory(string warehouseId)
    {
        var exclude = _isEditMode ? new InventoryCalculator.DocumentRef("Transfer", _transferId) : (InventoryCalculator.DocumentRef?)null;
        var all = InventoryCalculator.CalculateAll(_db, exclude);

        return all
            .Where(kv => kv.Key.WarehouseId == warehouseId)
            .ToDictionary(kv => kv.Key.ProductId, kv => kv.Value.Expected);
    }

    partial void OnSelectedSourceWarehouseChanged(CachedWarehouse? value)
    {
        UpdateAvailableProducts();
        UpdateTransferTypeLock();
    }

    partial void OnSelectedDestWarehouseChanged(CachedWarehouse? value)
    {
        UpdateTransferTypeLock();
    }

    private void UpdateAvailableProducts()
    {
        if (SelectedSourceWarehouse == null)
        {
            // Show all products
            foreach (var line in LineItems)
                line.AvailableProducts = Products;
            _warehouseInventory.Clear();
            return;
        }

        _warehouseInventory = CalculateWarehouseInventory(SelectedSourceWarehouse.Id);

        // Filter to products with qty > 0
        var available = new ObservableCollection<CachedProduct>(
            Products.Where(p => _warehouseInventory.TryGetValue(p.Id, out var qty) && qty > 0)
        );

        foreach (var line in LineItems)
        {
            line.AvailableProducts = available;
            // Clear selection if product no longer available
            if (line.SelectedProduct != null && !available.Any(p => p.Id == line.SelectedProduct.Id))
            {
                line.SelectedProduct = null;
                line.Quantity = 0;
            }
        }
    }

    private void UpdateTransferTypeLock()
    {
        var srcIsPhysical = SelectedSourceWarehouse?.Type == "Physical";
        var dstIsPhysical = SelectedDestWarehouse?.Type == "Physical";

        if (srcIsPhysical || dstIsPhysical)
        {
            TransferType = "Physical";
            IsTransferTypeLocked = true;
        }
        else
        {
            IsTransferTypeLocked = false;
        }
    }

    [RelayCommand]
    private void AddLine()
    {
        var available = SelectedSourceWarehouse != null
            ? new ObservableCollection<CachedProduct>(
                Products.Where(p => _warehouseInventory.TryGetValue(p.Id, out var qty) && qty > 0))
            : Products;

        LineItems.Add(new TransferLineItem
        {
            LineNumber = LineItems.Count + 1,
            AvailableProducts = available
        });
    }

    private void AddEmptyLine() => AddLine();

    [RelayCommand]
    private void RemoveLine(TransferLineItem? line)
    {
        if (line != null && LineItems.Count > 1)
        {
            LineItems.Remove(line);
            for (int i = 0; i < LineItems.Count; i++)
                LineItems[i].LineNumber = i + 1;
        }
    }

    [RelayCommand]
    private async Task ScanFromImage()
    {
        ScanStatusMessage = "Đang chọn ảnh...";
        var result = await _scanner.ScanFromImageAsync();
        if (result != null)
        {
            ProcessScannedData(result);
            ScanStatusMessage = "Đã quét ảnh thành công!";
        }
        else
        {
            ScanStatusMessage = "";
        }
    }

    [RelayCommand]
    private void ToggleHidScanner()
    {
        if (_scanner.IsHidListening)
        {
            _scanner.StopHidListening();
            IsHidListening = false;
            ScanStatusMessage = "";
        }
        else
        {
            _scanner.StartHidListening(data => 
            {
                ProcessScannedData(data);
                ScanStatusMessage = $"Vừa quét: {data}";
            });
            IsHidListening = true;
            ScanStatusMessage = "Đang lắng nghe máy quét USB...";
        }
    }

    private void ProcessScannedData(string data)
    {
        var line = LineItems.LastOrDefault();
        if (line != null && line.SelectedProduct != null && line.Quantity > 0)
        {
            AddEmptyLine();
            line = LineItems.Last();
        }

        var product = Products.FirstOrDefault(p => p.ProductCode.Equals(data, StringComparison.OrdinalIgnoreCase));
        if (line != null)
        {
            if (product != null)
            {
                line.SelectedProduct = product;
                line.Quantity = 1;
            }
        }
    }

    [RelayCommand]
    private void SaveDraft()
    {
        if (!Validate()) return;
        SaveTransfer("Draft");
        _nav.NavigateTo<TransferListViewModel>();
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!Validate()) return;
        if (!ValidateStockAvailability()) return;
        SaveTransfer("PendingSync");
        if (_scanner.IsHidListening) ToggleHidScanner();
        _nav.NavigateTo<TransferListViewModel>();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_scanner.IsHidListening) ToggleHidScanner();
        _nav.NavigateTo<TransferListViewModel>();
    }

    private bool Validate()
    {
        var validator = new TransferValidator();
        var result = validator.Validate(this);

        if (!result.IsValid)
        {
            ErrorMessage = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
            return false;
        }

        ErrorMessage = "";
        return true;
    }

    /// <summary>
    /// Blocks confirming a transfer that would push the source warehouse's expected quantity
    /// (snapshot + every other pending Receipt/Issue/Transfer) below zero for any product line.
    /// </summary>
    private bool ValidateStockAvailability()
    {
        if (SelectedSourceWarehouse == null) return true;

        var exclude = _isEditMode ? new InventoryCalculator.DocumentRef("Transfer", _transferId) : (InventoryCalculator.DocumentRef?)null;

        var neededByProduct = LineItems
            .Where(l => l.SelectedProduct != null && l.Quantity > 0)
            .GroupBy(l => l.SelectedProduct!)
            .Select(g => new { Product = g.Key, Qty = g.Sum(l => l.Quantity) });

        foreach (var need in neededByProduct)
        {
            var expected = InventoryCalculator.GetExpected(_db, SelectedSourceWarehouse.Id, need.Product.Id, exclude);
            if (expected - need.Qty < 0)
            {
                ErrorMessage = $"Không đủ tồn kho dự kiến cho '{need.Product.ProductName}' tại kho nguồn " +
                                $"(còn {expected:N0} {need.Product.Unit}, cần {need.Qty:N0}).";
                return false;
            }
        }

        return true;
    }

    private void SaveTransfer(string status)
    {
        var now = DateTime.UtcNow.ToString("o");

        if (_isEditMode)
        {
            // Update existing transfer
            var existing = _db.LocalTransfers.Include(t => t.Items).FirstOrDefault(t => t.Id == _transferId);
            if (existing == null) return;

            // Record edit history (snapshot old values)
            var oldSnapshot = JsonSerializer.Serialize(new
            {
                existing.SourceWarehouseId,
                existing.DestWarehouseId,
                existing.TransferType,
                existing.TransferDate,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            // Remove old items
            var oldItems = existing.Items.ToList();
            _db.LocalTransferItems.RemoveRange(oldItems);
            existing.Items.Clear();

            // Update header
            existing.SourceWarehouseId = SelectedSourceWarehouse!.Id;
            existing.DestWarehouseId = SelectedDestWarehouse!.Id;
            existing.TransferType = TransferType;
            existing.TransferDate = TransferDate.ToString("yyyy-MM-dd");
            existing.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
            existing.Status = status;
            existing.UpdatedAt = now;
            existing.Version++;

            // Add new items
            foreach (var line in LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0))
            {
                existing.Items.Add(new LocalTransferItem
                {
                    TransferId = existing.Id,
                    ProductId = line.SelectedProduct!.Id,
                    Quantity = line.Quantity,
                    Unit = line.SelectedProduct.Unit,
                    LotNumber = string.IsNullOrWhiteSpace(line.LotNumber) ? null : line.LotNumber,
                    BatchNumber = string.IsNullOrWhiteSpace(line.BatchNumber) ? null : line.BatchNumber
                });
            }

            var newSnapshot = JsonSerializer.Serialize(new
            {
                existing.SourceWarehouseId,
                existing.DestWarehouseId,
                existing.TransferType,
                existing.TransferDate,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _transferId,
                DocumentType = "Transfer",
                Action = "Edited",
                OldValues = oldSnapshot,
                NewValues = newSnapshot,
                ChangedBy = _currentUser,
                ChangedAt = now
            });

            _db.SaveChanges();
        }
        else
        {
            // Create new transfer
            var date = TransferDate.ToString("yyyy/MM/dd");
            var todayCount = _db.LocalTransfers
                .Count(t => t.TransferDate == TransferDate.ToString("yyyy-MM-dd")) + 1;
            var localNumber = $"LOCAL/LC/{date}/{todayCount:D3}";

            var transfer = new LocalTransfer
            {
                Id = _transferId,
                LocalNumber = localNumber,
                SourceWarehouseId = SelectedSourceWarehouse!.Id,
                DestWarehouseId = SelectedDestWarehouse!.Id,
                TransferType = TransferType,
                TransferDate = TransferDate.ToString("yyyy-MM-dd"),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                Status = status,
                SyncStatus = "Pending",
                CreatedBy = _currentUser,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var line in LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0))
            {
                transfer.Items.Add(new LocalTransferItem
                {
                    TransferId = transfer.Id,
                    ProductId = line.SelectedProduct!.Id,
                    Quantity = line.Quantity,
                    Unit = line.SelectedProduct.Unit,
                    LotNumber = string.IsNullOrWhiteSpace(line.LotNumber) ? null : line.LotNumber,
                    BatchNumber = string.IsNullOrWhiteSpace(line.BatchNumber) ? null : line.BatchNumber
                });
            }

            _db.LocalTransfers.Add(transfer);

            // Record creation in edit history
            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _transferId,
                DocumentType = "Transfer",
                Action = "Created",
                NewValues = JsonSerializer.Serialize(new
                {
                    transfer.SourceWarehouseId,
                    transfer.DestWarehouseId,
                    transfer.TransferType,
                    transfer.TransferDate,
                    transfer.Notes,
                    transfer.Status,
                    Items = transfer.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
                }),
                ChangedBy = _currentUser,
                ChangedAt = now
            });

            _db.SaveChanges();
        }
    }
}

public partial class TransferLineItem : ObservableObject
{
    [ObservableProperty] private int _lineNumber;
    [ObservableProperty] private CachedProduct? _selectedProduct;
    [ObservableProperty] private double _quantity;
    [ObservableProperty] private string _lotNumber = "";
    [ObservableProperty] private string _batchNumber = "";
    [ObservableProperty] private ObservableCollection<CachedProduct> _availableProducts = new();

    public string UnitDisplay => SelectedProduct?.Unit ?? "KG";
}
