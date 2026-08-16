using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels;

public partial class TransactionDetailViewModel : BaseViewModel, IParameterReceiver
{
    private readonly LocalDbContext _db;
    private readonly INavigationService _nav;

    [ObservableProperty] private string _documentId = "";
    [ObservableProperty] private string _documentType = "";
    [ObservableProperty] private string _documentNumber = "";
    [ObservableProperty] private string _documentDate = "";
    [ObservableProperty] private string _status = "";
    [ObservableProperty] private string _warehouseName = "";
    [ObservableProperty] private string _partnerName = ""; // Supplier or Customer
    [ObservableProperty] private string _partnerLabel = "Đối tác";
    [ObservableProperty] private string _notes = "";

    public ObservableCollection<DetailLineItem> LineItems { get; } = new();
    public ObservableCollection<EditHistoryItem> EditHistories { get; } = new();

    public TransactionDetailViewModel(LocalDbContext db, INavigationService nav)
    {
        _db = db;
        _nav = nav;
        Title = "Chi tiết chứng từ";
    }

    public void ReceiveParameter(object parameter)
    {
        if (parameter is TransactionDetailParams p)
        {
            DocumentId = p.DocumentId;
            DocumentType = p.DocumentType;
            LoadData();
        }
    }

    private void LoadData()
    {
        LineItems.Clear();
        EditHistories.Clear();

        if (DocumentType == "Receipt")
        {
            Title = "Chi tiết Phiếu Nhập Kho";
            PartnerLabel = "Nhà cung cấp";
            LoadReceipt();
        }
        else if (DocumentType == "Issue")
        {
            Title = "Chi tiết Phiếu Xuất Kho";
            PartnerLabel = "Khách hàng";
            LoadIssue();
        }
        else if (DocumentType == "Transfer")
        {
            Title = "Chi tiết Luân Chuyển Kho";
            PartnerLabel = "Kho đích";
            LoadTransfer();
        }

        LoadEditHistory();
    }

    private void LoadReceipt()
    {
        var receipt = _db.LocalStockReceipts.Include(r => r.Items).FirstOrDefault(r => r.Id == DocumentId);
        if (receipt == null) return;

        DocumentNumber = receipt.LocalNumber;
        DocumentDate = receipt.ReceiptDate;
        Status = receipt.Status;
        Notes = receipt.Notes ?? "";
        
        var wh = _db.CachedWarehouses.FirstOrDefault(w => w.Id == receipt.WarehouseId);
        WarehouseName = wh?.Name ?? receipt.WarehouseId;

        var supplier = _db.CachedSuppliers.FirstOrDefault(s => s.Id == receipt.SupplierId);
        PartnerName = supplier?.Name ?? "";

        foreach (var item in receipt.Items)
        {
            var p = _db.CachedProducts.FirstOrDefault(x => x.Id == item.ProductId);
            LineItems.Add(new DetailLineItem
            {
                ProductCode = p?.ProductCode ?? item.ProductId,
                ProductName = p?.ProductName ?? "",
                Quantity = item.Quantity,
                Unit = item.Unit,
                LotNumber = item.LotNumber ?? "",
                BatchNumber = item.BatchNumber ?? ""
            });
        }
    }

    private void LoadIssue()
    {
        var issue = _db.LocalStockIssues.Include(i => i.Items).FirstOrDefault(i => i.Id == DocumentId);
        if (issue == null) return;

        DocumentNumber = issue.LocalNumber;
        DocumentDate = issue.IssueDate;
        Status = issue.Status;
        Notes = issue.Notes ?? "";

        var wh = _db.CachedWarehouses.FirstOrDefault(w => w.Id == issue.WarehouseId);
        WarehouseName = wh?.Name ?? issue.WarehouseId;

        var customer = _db.CachedCustomers.FirstOrDefault(c => c.Id == issue.CustomerId);
        PartnerName = customer?.Name ?? "";

        foreach (var item in issue.Items)
        {
            var p = _db.CachedProducts.FirstOrDefault(x => x.Id == item.ProductId);
            LineItems.Add(new DetailLineItem
            {
                ProductCode = p?.ProductCode ?? item.ProductId,
                ProductName = p?.ProductName ?? "",
                Quantity = item.Quantity,
                Unit = item.Unit,
                LotNumber = item.LotNumber ?? "",
                BatchNumber = item.BatchNumber ?? ""
            });
        }
    }

    private void LoadTransfer()
    {
        var transfer = _db.LocalTransfers.Include(t => t.Items).FirstOrDefault(t => t.Id == DocumentId);
        if (transfer == null) return;

        DocumentNumber = transfer.LocalNumber;
        DocumentDate = transfer.TransferDate;
        Status = transfer.Status;
        Notes = transfer.Notes ?? "";

        var whSrc = _db.CachedWarehouses.FirstOrDefault(w => w.Id == transfer.SourceWarehouseId);
        WarehouseName = whSrc?.Name ?? transfer.SourceWarehouseId;

        var whDst = _db.CachedWarehouses.FirstOrDefault(w => w.Id == transfer.DestWarehouseId);
        PartnerName = whDst?.Name ?? transfer.DestWarehouseId;

        foreach (var item in transfer.Items)
        {
            var p = _db.CachedProducts.FirstOrDefault(x => x.Id == item.ProductId);
            LineItems.Add(new DetailLineItem
            {
                ProductCode = p?.ProductCode ?? item.ProductId,
                ProductName = p?.ProductName ?? "",
                Quantity = item.Quantity,
                Unit = item.Unit,
                LotNumber = item.LotNumber ?? "",
                BatchNumber = item.BatchNumber ?? ""
            });
        }
    }

    private void LoadEditHistory()
    {
        var histories = _db.EditHistories
            .Where(h => h.DocumentId == DocumentId)
            .OrderByDescending(h => h.ChangedAt)
            .ToList();

        foreach (var h in histories)
        {
            var item = new EditHistoryItem
            {
                Action = TranslateAction(h.Action),
                ChangedBy = h.ChangedBy,
                ChangedAt = DateTime.TryParse(h.ChangedAt, out var dt) ? dt.ToString("dd/MM/yyyy HH:mm:ss") : h.ChangedAt,
                Notes = h.Notes ?? ""
            };

            if (h.Action == "Created" && !string.IsNullOrEmpty(h.NewValues))
            {
                ParseDifferences("{}", h.NewValues, item.Changes);
            }
            else if (h.Action == "Edited" && !string.IsNullOrEmpty(h.OldValues) && !string.IsNullOrEmpty(h.NewValues))
            {
                ParseDifferences(h.OldValues, h.NewValues, item.Changes);
            }

            EditHistories.Add(item);
        }
    }

    private void ParseDifferences(string oldJson, string newJson, ObservableCollection<ChangeDetail> changes)
    {
        try
        {
            using var oldDoc = JsonDocument.Parse(oldJson);
            using var newDoc = JsonDocument.Parse(newJson);

            var oldRoot = oldDoc.RootElement;
            var newRoot = newDoc.RootElement;

            foreach (var prop in newRoot.EnumerateObject())
            {
                if (prop.Name == "Items") continue;

                var oldProp = oldRoot.TryGetProperty(prop.Name, out var op) ? op : default;
                var oldVal = GetStringValue(oldProp);
                var newVal = GetStringValue(prop.Value);

                if (oldVal != newVal)
                {
                    changes.Add(new ChangeDetail
                    {
                        PropertyName = TranslateProperty(prop.Name),
                        OldValue = ResolveName(prop.Name, oldVal),
                        NewValue = ResolveName(prop.Name, newVal)
                    });
                }
            }

            var oldItems = oldRoot.TryGetProperty("Items", out var oi) ? oi : default;
            var newItems = newRoot.TryGetProperty("Items", out var ni) ? ni : default;
            CompareItems(oldItems, newItems, changes);
        }
        catch { }
    }

    private string GetStringValue(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined) return "";
        return el.ToString();
    }

    private string TranslateProperty(string name)
    {
        return name switch
        {
            "SourceWarehouseId" => "Kho nguồn",
            "WarehouseId" => "Kho",
            "DestWarehouseId" => "Kho đích",
            "SupplierId" => "Nhà cung cấp",
            "CustomerId" => "Khách hàng",
            "ReceiptDate" => "Ngày nhập",
            "IssueDate" => "Ngày xuất",
            "TransferDate" => "Ngày luân chuyển",
            "Status" => "Trạng thái",
            "Notes" => "Ghi chú",
            "ContractNumber" => "Số hợp đồng",
            "RoNumber" => "Số RO",
            "DriverName" => "Tài xế",
            "VehicleNumber" => "Số xe",
            _ => name
        };
    }

    private string ResolveName(string propertyName, string id)
    {
        if (string.IsNullOrEmpty(id)) return "Trống";
        if (propertyName.Contains("WarehouseId"))
            return _db.CachedWarehouses.FirstOrDefault(w => w.Id == id)?.Name ?? id;
        if (propertyName == "SupplierId")
            return _db.CachedSuppliers.FirstOrDefault(s => s.Id == id)?.Name ?? id;
        if (propertyName == "CustomerId")
            return _db.CachedCustomers.FirstOrDefault(c => c.Id == id)?.Name ?? id;
        
        if (propertyName.Contains("Date") && DateTime.TryParse(id, out var dt))
            return dt.ToString("dd/MM/yyyy");

        return id;
    }

    private void CompareItems(JsonElement oldItems, JsonElement newItems, ObservableCollection<ChangeDetail> changes)
    {
        // A brand-new document's "old" snapshot is "{}" (no Items property at all), so oldItems
        // is a default/Undefined JsonElement here, not an empty array - EnumerateArray() throws
        // on anything that isn't JsonValueKind.Array. Treat "not an array" as "no items" instead.
        var oldArray = oldItems.ValueKind == JsonValueKind.Array ? oldItems.EnumerateArray().ToList() : new List<JsonElement>();
        var newArray = newItems.ValueKind == JsonValueKind.Array ? newItems.EnumerateArray().ToList() : new List<JsonElement>();

        var allProductIds = oldArray.Select(x => x.TryGetProperty("ProductId", out var p) ? p.GetString() : null)
            .Union(newArray.Select(x => x.TryGetProperty("ProductId", out var p) ? p.GetString() : null))
            .Where(x => !string.IsNullOrEmpty(x));

        foreach (var pid in allProductIds.Distinct())
        {
            try
            {
                var pName = _db.CachedProducts.FirstOrDefault(p => p.Id == pid)?.ProductName ?? pid;

                var oldItem = oldArray.FirstOrDefault(x => x.TryGetProperty("ProductId", out var p) && p.GetString() == pid);
                var newItem = newArray.FirstOrDefault(x => x.TryGetProperty("ProductId", out var p) && p.GetString() == pid);

                var oldQty = oldItem.ValueKind != JsonValueKind.Undefined && oldItem.TryGetProperty("Quantity", out var oq) ? oq.GetDouble() : 0;
                var newQty = newItem.ValueKind != JsonValueKind.Undefined && newItem.TryGetProperty("Quantity", out var nq) ? nq.GetDouble() : 0;

                if (oldQty != newQty)
                {
                    changes.Add(new ChangeDetail
                    {
                        PropertyName = $"Sản phẩm '{pName}' (SL)",
                        OldValue = oldQty == 0 ? "Chưa có" : oldQty.ToString("N0"),
                        NewValue = newQty == 0 ? "Đã xóa" : newQty.ToString("N0")
                    });
                }

                var oldLot = oldItem.ValueKind != JsonValueKind.Undefined && oldItem.TryGetProperty("LotNumber", out var ol) ? GetStringValue(ol) : "";
                var newLot = newItem.ValueKind != JsonValueKind.Undefined && newItem.TryGetProperty("LotNumber", out var nl) ? GetStringValue(nl) : "";
                if (oldLot != newLot && newQty > 0)
                {
                    changes.Add(new ChangeDetail
                    {
                        PropertyName = $"Sản phẩm '{pName}' (Số lô)",
                        OldValue = string.IsNullOrEmpty(oldLot) ? "Trống" : oldLot,
                        NewValue = string.IsNullOrEmpty(newLot) ? "Trống" : newLot
                    });
                }

                var oldBatch = oldItem.ValueKind != JsonValueKind.Undefined && oldItem.TryGetProperty("BatchNumber", out var ob) ? GetStringValue(ob) : "";
                var newBatch = newItem.ValueKind != JsonValueKind.Undefined && newItem.TryGetProperty("BatchNumber", out var nb) ? GetStringValue(nb) : "";
                if (oldBatch != newBatch && newQty > 0)
                {
                    changes.Add(new ChangeDetail
                    {
                        PropertyName = $"Sản phẩm '{pName}' (Số Batch)",
                        OldValue = string.IsNullOrEmpty(oldBatch) ? "Trống" : oldBatch,
                        NewValue = string.IsNullOrEmpty(newBatch) ? "Trống" : newBatch
                    });
                }
            }
            catch { }
        }
    }

    private string TranslateAction(string action)
    {
        return action switch
        {
            "Created" => "Tạo mới",
            "Edited" => "Chỉnh sửa",
            "StatusChanged" => "Đổi trạng thái",
            "Synced" => "Đồng bộ",
            _ => action
        };
    }

    [RelayCommand]
    private void Close()
    {
        if (DocumentType == "Receipt") _nav.NavigateTo<Receipts.ReceiptListViewModel>();
        else if (DocumentType == "Issue") _nav.NavigateTo<Issues.IssueListViewModel>();
        else if (DocumentType == "Transfer") _nav.NavigateTo<Transfers.TransferListViewModel>();
    }
}

public class TransactionDetailParams
{
    public string DocumentId { get; set; } = "";
    public string DocumentType { get; set; } = ""; // "Receipt" | "Issue" | "Transfer"
}

public class DetailLineItem
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public double Quantity { get; set; }
    public string Unit { get; set; } = "";
    public string LotNumber { get; set; } = "";
    public string BatchNumber { get; set; } = "";
}

public class EditHistoryItem
{
    public string Action { get; set; } = "";
    public string ChangedBy { get; set; } = "";
    public string ChangedAt { get; set; } = "";
    public string Notes { get; set; } = "";
    
    public ObservableCollection<ChangeDetail> Changes { get; } = new();
    public bool HasChanges => Changes.Count > 0;
}

public class ChangeDetail
{
    public string PropertyName { get; set; } = "";
    public string OldValue { get; set; } = "";
    public string NewValue { get; set; } = "";
}
