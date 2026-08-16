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

namespace VjWms.Desktop.UI.ViewModels.Receipts;

public partial class ReceiptCreateViewModel : BaseViewModel, IParameterReceiver
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;
    private readonly IScannerService _scanner;
    private readonly string _currentUser;
    private string _receiptId;
    private bool _isEditMode;

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

    // Attachments
    public AttachmentPanelViewModel AttachmentPanel { get; }

    // Scanner
    [ObservableProperty] private bool _isHidListening;
    [ObservableProperty] private string _scanStatusMessage = "";

    public ReceiptCreateViewModel(LocalDbContext db, INavigationService nav, IScannerService scanner, AttachmentService attachmentService)
    {
        _db = db;
        _nav = nav;
        _scanner = scanner;
        _currentUser = "admin"; // TODO: inject current user
        _receiptId = Guid.NewGuid().ToString(); // Pre-generate ID for attachments
        Title = "Tạo Phiếu Nhập Kho";

        AttachmentPanel = new AttachmentPanelViewModel(attachmentService, _receiptId, "Receipt");

        LoadMasterData();
        AddEmptyLine();
    }

    public void ReceiveParameter(object parameter)
    {
        if (parameter is string receiptId)
        {
            LoadExistingReceipt(receiptId);
        }
    }

    private void LoadExistingReceipt(string receiptId)
    {
        var receipt = _db.LocalStockReceipts
            .Include(r => r.Items)
            .FirstOrDefault(r => r.Id == receiptId);

        if (receipt == null) return;

        _isEditMode = true;
        _receiptId = receiptId;
        Title = "Chỉnh sửa Phiếu Nhập Kho";

        SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == receipt.WarehouseId);
        SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == receipt.SupplierId);
        ReceiptDate = DateTime.TryParse(receipt.ReceiptDate, out var dt) ? dt : DateTime.Today;
        ContractNumber = receipt.ContractNumber ?? "";
        RoNumber = receipt.RoNumber ?? "";
        DriverName = receipt.DriverName ?? "";
        DriverIdCard = receipt.DriverIdCard ?? "";
        VehicleNumber = receipt.VehicleNumber ?? "";
        TrailerNumber = receipt.TrailerNumber ?? "";
        Notes = receipt.Notes ?? "";

        LineItems.Clear();
        int lineNum = 1;
        foreach (var item in receipt.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            LineItems.Add(new ReceiptLineItem
            {
                LineNumber = lineNum++,
                SelectedProduct = product,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice ?? 0,
                LotNumber = item.LotNumber ?? "",
                BatchNumber = item.BatchNumber ?? "",
                NumberOfBags = item.NumberOfBags ?? 0,
                PackagingType = item.PackagingType,
                LineNotes = item.Notes ?? "",
                AvailableProducts = Products
            });
        }

        if (LineItems.Count == 0) AddEmptyLine();
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
        // Simple logic: assume data is ProductCode
        // Find if we have an empty line
        var line = LineItems.LastOrDefault();
        if (line != null && line.SelectedProduct != null && line.Quantity > 0)
        {
            // Add a new line if the last one is already filled
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
            else
            {
                // Just put it in the notes if we can't find it
                line.LineNotes = $"Scanned: {data}";
            }
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
        if (_scanner.IsHidListening) ToggleHidScanner(); // Stop listening
        _nav.NavigateTo<ReceiptListViewModel>();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_scanner.IsHidListening) ToggleHidScanner(); // Stop listening
        _nav.NavigateTo<ReceiptListViewModel>();
    }

    private bool Validate()
    {
        var validator = new ReceiptValidator();
        var result = validator.Validate(this);

        if (!result.IsValid)
        {
            ErrorMessage = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
            return false;
        }

        ErrorMessage = "";
        return true;
    }

    private void SaveReceipt(string status)
    {
        var now = DateTime.UtcNow.ToString("o");
        var date = ReceiptDate.ToString("yyyy-MM-dd");

        if (_isEditMode)
        {
            var existing = _db.LocalStockReceipts.Include(r => r.Items).FirstOrDefault(r => r.Id == _receiptId);
            if (existing == null) return;

            var oldSnapshot = JsonSerializer.Serialize(new
            {
                existing.WarehouseId,
                existing.SupplierId,
                existing.ReceiptDate,
                existing.ContractNumber,
                existing.RoNumber,
                existing.DriverName,
                existing.VehicleNumber,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            var oldItems = existing.Items.ToList();
            _db.LocalStockReceiptItems.RemoveRange(oldItems);
            existing.Items.Clear();

            existing.WarehouseId = SelectedWarehouse!.Id;
            existing.SupplierId = SelectedSupplier?.Id;
            existing.ReceiptDate = date;
            existing.ContractNumber = string.IsNullOrWhiteSpace(ContractNumber) ? null : ContractNumber;
            existing.RoNumber = string.IsNullOrWhiteSpace(RoNumber) ? null : RoNumber;
            existing.DriverName = string.IsNullOrWhiteSpace(DriverName) ? null : DriverName;
            existing.DriverIdCard = string.IsNullOrWhiteSpace(DriverIdCard) ? null : DriverIdCard;
            existing.VehicleNumber = string.IsNullOrWhiteSpace(VehicleNumber) ? null : VehicleNumber;
            existing.TrailerNumber = string.IsNullOrWhiteSpace(TrailerNumber) ? null : TrailerNumber;
            existing.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes;
            existing.Status = status;
            existing.UpdatedAt = now;
            existing.Version++;

            foreach (var line in LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0))
            {
                existing.Items.Add(new LocalStockReceiptItem
                {
                    ReceiptId = existing.Id,
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

            var newSnapshot = JsonSerializer.Serialize(new
            {
                existing.WarehouseId,
                existing.SupplierId,
                existing.ReceiptDate,
                existing.ContractNumber,
                existing.RoNumber,
                existing.DriverName,
                existing.VehicleNumber,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _receiptId,
                DocumentType = "Receipt",
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
            var dateSegment = ReceiptDate.ToString("yyyy/MM/dd");
            var todayCount = _db.LocalStockReceipts
                .Count(r => r.ReceiptDate == date) + 1;
            var localNumber = $"LOCAL/NK/{dateSegment}/{todayCount:D3}";

            var receipt = new LocalStockReceipt
            {
                Id = _receiptId,
                LocalNumber = localNumber,
                WarehouseId = SelectedWarehouse!.Id,
                SupplierId = SelectedSupplier?.Id,
                ReceiptDate = date,
                ContractNumber = string.IsNullOrWhiteSpace(ContractNumber) ? null : ContractNumber,
                RoNumber = string.IsNullOrWhiteSpace(RoNumber) ? null : RoNumber,
                DriverName = string.IsNullOrWhiteSpace(DriverName) ? null : DriverName,
                DriverIdCard = string.IsNullOrWhiteSpace(DriverIdCard) ? null : DriverIdCard,
                VehicleNumber = string.IsNullOrWhiteSpace(VehicleNumber) ? null : VehicleNumber,
                TrailerNumber = string.IsNullOrWhiteSpace(TrailerNumber) ? null : TrailerNumber,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                Status = status,
                SyncStatus = "Pending",
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

            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _receiptId,
                DocumentType = "Receipt",
                Action = "Created",
                NewValues = JsonSerializer.Serialize(new
                {
                    receipt.WarehouseId,
                    receipt.SupplierId,
                    receipt.ReceiptDate,
                    receipt.ContractNumber,
                    receipt.RoNumber,
                    receipt.DriverName,
                    receipt.DriverIdCard,
                    receipt.VehicleNumber,
                    receipt.TrailerNumber,
                    receipt.Notes,
                    receipt.Status,
                    Items = receipt.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
                }),
                ChangedBy = _currentUser,
                ChangedAt = now
            });

            _db.SaveChanges();
        }
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
