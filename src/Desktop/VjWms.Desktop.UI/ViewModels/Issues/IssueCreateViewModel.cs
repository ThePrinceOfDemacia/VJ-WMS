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

namespace VjWms.Desktop.UI.ViewModels.Issues;

public partial class IssueCreateViewModel : BaseViewModel, IParameterReceiver
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;
    private readonly IScannerService _scanner;
    private readonly string _currentUser;
    private string _issueId;
    private bool _isEditMode;

    // Header fields
    [ObservableProperty] private CachedWarehouse? _selectedWarehouse;
    [ObservableProperty] private CachedCustomer? _selectedCustomer;
    [ObservableProperty] private DateTime _issueDate = DateTime.Today;
    [ObservableProperty] private string _contractNumber = "";
    [ObservableProperty] private string _roNumber = "";
    [ObservableProperty] private string _issueReason = "";
    [ObservableProperty] private string _deliveryTerm = "";
    [ObservableProperty] private string _containerNo = "";
    [ObservableProperty] private string _sealNo = "";
    [ObservableProperty] private string _bookingNo = "";
    [ObservableProperty] private string _driverName = "";
    [ObservableProperty] private string _driverIdCard = "";
    [ObservableProperty] private string _vehicleNumber = "";
    [ObservableProperty] private string _trailerNumber = "";
    [ObservableProperty] private string _notes = "";
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<CachedWarehouse> Warehouses { get; } = new();
    public ObservableCollection<CachedCustomer> Customers { get; } = new();
    public ObservableCollection<CachedProduct> Products { get; } = new();
    public ObservableCollection<IssueLineItem> LineItems { get; } = new();

    // Attachments
    public AttachmentPanelViewModel AttachmentPanel { get; }

    // Scanner
    [ObservableProperty] private bool _isHidListening;
    [ObservableProperty] private string _scanStatusMessage = "";

    public IssueCreateViewModel(LocalDbContext db, INavigationService nav, IScannerService scanner, AttachmentService attachmentService)
    {
        _db = db;
        _nav = nav;
        _scanner = scanner;
        _currentUser = "admin";
        _issueId = Guid.NewGuid().ToString();
        Title = "Tạo Phiếu Xuất Kho";

        AttachmentPanel = new AttachmentPanelViewModel(attachmentService, _issueId, "Issue");

        LoadMasterData();
        AddEmptyLine();
    }

    public void ReceiveParameter(object parameter)
    {
        if (parameter is string issueId)
        {
            LoadExistingIssue(issueId);
        }
    }

    private void LoadExistingIssue(string issueId)
    {
        var issue = _db.LocalStockIssues
            .Include(i => i.Items)
            .FirstOrDefault(i => i.Id == issueId);

        if (issue == null) return;

        _isEditMode = true;
        _issueId = issueId;
        Title = "Chỉnh sửa Phiếu Xuất Kho";

        SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == issue.WarehouseId);
        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == issue.CustomerId);
        IssueDate = DateTime.TryParse(issue.IssueDate, out var dt) ? dt : DateTime.Today;
        ContractNumber = issue.ContractNumber ?? "";
        RoNumber = issue.RoNumber ?? "";
        IssueReason = issue.IssueReason ?? "";
        DeliveryTerm = issue.DeliveryTerm ?? "";
        ContainerNo = issue.ContainerNo ?? "";
        SealNo = issue.SealNo ?? "";
        BookingNo = issue.BookingNo ?? "";
        DriverName = issue.DriverName ?? "";
        DriverIdCard = issue.DriverIdCard ?? "";
        VehicleNumber = issue.VehicleNumber ?? "";
        TrailerNumber = issue.TrailerNumber ?? "";
        Notes = issue.Notes ?? "";

        LineItems.Clear();
        int lineNum = 1;
        foreach (var item in issue.Items)
        {
            var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
            LineItems.Add(new IssueLineItem
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
        
        UpdateAvailableProducts();
    }

    private void LoadMasterData()
    {
        foreach (var w in _db.CachedWarehouses.Where(w => w.IsActive).ToList())
            Warehouses.Add(w);
        foreach (var c in _db.CachedCustomers.ToList())
            Customers.Add(c);
        foreach (var p in _db.CachedProducts.Where(p => p.IsActive).ToList())
            Products.Add(p);
    }

    private Dictionary<string, double> _warehouseInventory = new();

    private Dictionary<string, double> CalculateWarehouseInventory(string warehouseId)
    {
        var exclude = _isEditMode ? new InventoryCalculator.DocumentRef("Issue", _issueId) : (InventoryCalculator.DocumentRef?)null;
        var all = InventoryCalculator.CalculateAll(_db, exclude);

        return all
            .Where(kv => kv.Key.WarehouseId == warehouseId)
            .ToDictionary(kv => kv.Key.ProductId, kv => kv.Value.Expected);
    }

    partial void OnSelectedWarehouseChanged(CachedWarehouse? value)
    {
        UpdateAvailableProducts();
    }

    private void UpdateAvailableProducts()
    {
        if (SelectedWarehouse == null)
        {
            foreach (var line in LineItems)
                line.AvailableProducts = Products;
            _warehouseInventory.Clear();
            return;
        }

        _warehouseInventory = CalculateWarehouseInventory(SelectedWarehouse.Id);

        var available = new ObservableCollection<CachedProduct>(
            Products.Where(p => _warehouseInventory.TryGetValue(p.Id, out var qty) && qty > 0)
        );

        foreach (var line in LineItems)
        {
            line.AvailableProducts = available;
            if (line.SelectedProduct != null && !available.Any(p => p.Id == line.SelectedProduct.Id))
            {
                line.SelectedProduct = null;
                line.Quantity = 0;
            }
        }
    }

    [RelayCommand]
    public void AddLine()
    {
        var available = SelectedWarehouse != null 
            ? new ObservableCollection<CachedProduct>(Products.Where(p => _warehouseInventory.TryGetValue(p.Id, out var qty) && qty > 0))
            : Products;

        LineItems.Add(new IssueLineItem
        {
            LineNumber = LineItems.Count + 1,
            AvailableProducts = available
        });
    }

    private void AddEmptyLine() => AddLine();

    [RelayCommand]
    private void RemoveLine(IssueLineItem? line)
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
            else
            {
                line.LineNotes = $"Scanned: {data}";
            }
        }
    }

    [RelayCommand]
    private void SaveDraft()
    {
        if (!Validate()) return;
        SaveIssue("Draft");
        _nav.NavigateTo<IssueListViewModel>();
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!Validate()) return;
        if (!ValidateStockAvailability()) return;
        SaveIssue("PendingSync");
        if (_scanner.IsHidListening) ToggleHidScanner();
        _nav.NavigateTo<IssueListViewModel>();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_scanner.IsHidListening) ToggleHidScanner();
        _nav.NavigateTo<IssueListViewModel>();
    }

    private bool Validate()
    {
        var validator = new IssueValidator();
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
    /// Blocks confirming an issue that would push the warehouse's expected quantity (snapshot +
    /// every other pending Receipt/Issue/Transfer) below zero for any product line.
    /// </summary>
    private bool ValidateStockAvailability()
    {
        if (SelectedWarehouse == null) return true;

        var exclude = _isEditMode ? new InventoryCalculator.DocumentRef("Issue", _issueId) : (InventoryCalculator.DocumentRef?)null;

        var neededByProduct = LineItems
            .Where(l => l.SelectedProduct != null && l.Quantity > 0)
            .GroupBy(l => l.SelectedProduct!)
            .Select(g => new { Product = g.Key, Qty = g.Sum(l => l.Quantity) });

        foreach (var need in neededByProduct)
        {
            var expected = InventoryCalculator.GetExpected(_db, SelectedWarehouse.Id, need.Product.Id, exclude);
            if (expected - need.Qty < 0)
            {
                ErrorMessage = $"Không đủ tồn kho dự kiến cho '{need.Product.ProductName}' " +
                                $"(còn {expected:N0} {need.Product.Unit}, cần {need.Qty:N0}).";
                return false;
            }
        }

        return true;
    }

    private void SaveIssue(string status)
    {
        var now = DateTime.UtcNow.ToString("o");
        var date = IssueDate.ToString("yyyy-MM-dd");

        if (_isEditMode)
        {
            var existing = _db.LocalStockIssues.Include(i => i.Items).FirstOrDefault(i => i.Id == _issueId);
            if (existing == null) return;

            var oldSnapshot = JsonSerializer.Serialize(new
            {
                existing.WarehouseId,
                existing.CustomerId,
                existing.IssueDate,
                existing.ContractNumber,
                existing.RoNumber,
                existing.IssueReason,
                existing.DeliveryTerm,
                existing.ContainerNo,
                existing.SealNo,
                existing.BookingNo,
                existing.DriverName,
                existing.VehicleNumber,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            var oldItems = existing.Items.ToList();
            _db.LocalStockIssueItems.RemoveRange(oldItems);
            existing.Items.Clear();

            existing.WarehouseId = SelectedWarehouse!.Id;
            existing.CustomerId = SelectedCustomer?.Id;
            existing.IssueDate = date;
            existing.ContractNumber = string.IsNullOrWhiteSpace(ContractNumber) ? null : ContractNumber;
            existing.RoNumber = string.IsNullOrWhiteSpace(RoNumber) ? null : RoNumber;
            existing.IssueReason = string.IsNullOrWhiteSpace(IssueReason) ? null : IssueReason;
            existing.DeliveryTerm = string.IsNullOrWhiteSpace(DeliveryTerm) ? null : DeliveryTerm;
            existing.ContainerNo = string.IsNullOrWhiteSpace(ContainerNo) ? null : ContainerNo;
            existing.SealNo = string.IsNullOrWhiteSpace(SealNo) ? null : SealNo;
            existing.BookingNo = string.IsNullOrWhiteSpace(BookingNo) ? null : BookingNo;
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
                existing.Items.Add(new LocalStockIssueItem
                {
                    IssueId = existing.Id,
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
                existing.CustomerId,
                existing.IssueDate,
                existing.ContractNumber,
                existing.RoNumber,
                existing.IssueReason,
                existing.DeliveryTerm,
                existing.ContainerNo,
                existing.SealNo,
                existing.BookingNo,
                existing.DriverName,
                existing.VehicleNumber,
                existing.Notes,
                existing.Status,
                Items = existing.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
            });

            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _issueId,
                DocumentType = "Issue",
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
            var dateSegment = IssueDate.ToString("yyyy/MM/dd");
            var todayCount = _db.LocalStockIssues
                .Count(i => i.IssueDate == date) + 1;
            var localNumber = $"LOCAL/XK/{dateSegment}/{todayCount:D3}";

            var issue = new LocalStockIssue
            {
                Id = _issueId,
                LocalNumber = localNumber,
                WarehouseId = SelectedWarehouse!.Id,
                CustomerId = SelectedCustomer?.Id,
                IssueDate = date,
                ContractNumber = string.IsNullOrWhiteSpace(ContractNumber) ? null : ContractNumber,
                RoNumber = string.IsNullOrWhiteSpace(RoNumber) ? null : RoNumber,
                IssueReason = string.IsNullOrWhiteSpace(IssueReason) ? null : IssueReason,
                DeliveryTerm = string.IsNullOrWhiteSpace(DeliveryTerm) ? null : DeliveryTerm,
                ContainerNo = string.IsNullOrWhiteSpace(ContainerNo) ? null : ContainerNo,
                SealNo = string.IsNullOrWhiteSpace(SealNo) ? null : SealNo,
                BookingNo = string.IsNullOrWhiteSpace(BookingNo) ? null : BookingNo,
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
                issue.Items.Add(new LocalStockIssueItem
                {
                    IssueId = issue.Id,
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

            _db.LocalStockIssues.Add(issue);

            _db.EditHistories.Add(new LocalEditHistory
            {
                DocumentId = _issueId,
                DocumentType = "Issue",
                Action = "Created",
                NewValues = JsonSerializer.Serialize(new
                {
                    issue.WarehouseId,
                    issue.CustomerId,
                    issue.IssueDate,
                    issue.ContractNumber,
                    issue.RoNumber,
                    issue.IssueReason,
                    issue.DeliveryTerm,
                    issue.ContainerNo,
                    issue.SealNo,
                    issue.BookingNo,
                    issue.DriverName,
                    issue.VehicleNumber,
                    issue.Notes,
                    issue.Status,
                    Items = issue.Items.Select(i => new { i.ProductId, i.Quantity, i.LotNumber, i.BatchNumber })
                }),
                ChangedBy = _currentUser,
                ChangedAt = now
            });

            _db.SaveChanges();
        }
    }
}

public partial class IssueLineItem : ObservableObject
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

    [ObservableProperty] private ObservableCollection<CachedProduct> _availableProducts = new();
    public string UnitDisplay => SelectedProduct?.Unit ?? "KG";
}
