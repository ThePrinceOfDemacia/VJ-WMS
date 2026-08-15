using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Issues;

public partial class IssueCreateViewModel : BaseViewModel
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;
    private readonly string _currentUser;

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

    public IssueCreateViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        _currentUser = "admin";
        Title = "Tạo Phiếu Xuất Kho";

        LoadMasterData();
        AddEmptyLine();
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

    [RelayCommand]
    private void AddLine()
    {
        LineItems.Add(new IssueLineItem
        {
            LineNumber = LineItems.Count + 1,
            AvailableProducts = Products
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
        SaveIssue("PendingSync");
        _nav.NavigateTo<IssueListViewModel>();
    }

    [RelayCommand]
    private void Cancel() => _nav.NavigateTo<IssueListViewModel>();

    private bool Validate()
    {
        if (SelectedWarehouse == null)
        {
            ErrorMessage = "Vui lòng chọn kho xuất!";
            return false;
        }

        var validLines = LineItems.Where(l => l.SelectedProduct != null && l.Quantity > 0).ToList();
        if (validLines.Count == 0)
        {
            ErrorMessage = "Cần ít nhất 1 dòng hàng hóa!";
            return false;
        }

        ErrorMessage = "";
        return true;
    }

    private void SaveIssue(string status)
    {
        var now = DateTime.UtcNow.ToString("o");
        var date = IssueDate.ToString("yyyy/MM/dd");
        var todayCount = _db.LocalStockIssues
            .Count(i => i.IssueDate == IssueDate.ToString("yyyy-MM-dd")) + 1;
        var localNumber = $"LOCAL/XK/{date}/{todayCount:D3}";

        var issue = new LocalStockIssue
        {
            LocalNumber = localNumber,
            WarehouseId = SelectedWarehouse!.Id,
            CustomerId = SelectedCustomer?.Id,
            IssueDate = IssueDate.ToString("yyyy-MM-dd"),
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
        _db.SaveChanges();
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

    public ObservableCollection<CachedProduct> AvailableProducts { get; set; } = new();
    public string UnitDisplay => SelectedProduct?.Unit ?? "KG";
}
