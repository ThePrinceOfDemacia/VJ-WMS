namespace VjWms.Desktop.Domain.Entities;

// ============================================================
// CACHED MASTER DATA (downloaded from server)
// ============================================================

public class CachedWarehouse
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string LastSyncedAt { get; set; } = string.Empty;
}

public class CachedProduct
{
    public string Id { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string LastSyncedAt { get; set; } = string.Empty;
}

public class CachedSupplier
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastSyncedAt { get; set; } = string.Empty;
}

public class CachedCustomer
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastSyncedAt { get; set; } = string.Empty;
}

public class CachedUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Permissions { get; set; } = "[]";
    public string PreferredLanguage { get; set; } = "vi-VN";
    public string LastSyncedAt { get; set; } = string.Empty;
}

public class CachedInventory
{
    public string Id { get; set; } = string.Empty;
    public string WarehouseId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? LocationId { get; set; }
    public double Quantity { get; set; }
    public double ReservedQuantity { get; set; }
    public string SnapshotAt { get; set; } = string.Empty;
}

// ============================================================
// LOCAL TRANSACTIONS (created offline, synced to server)
// ============================================================

public class LocalStockReceipt
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LocalNumber { get; set; } = string.Empty;
    public string? OfficialNumber { get; set; }
    public string WarehouseId { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public string ReceiptDate { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public string? RoNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverIdCard { get; set; }
    public string? VehicleNumber { get; set; }
    public string? TrailerNumber { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public string SyncStatus { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsReadOnly { get; set; } = false;

    public ICollection<LocalStockReceiptItem> Items { get; set; } = new List<LocalStockReceiptItem>();
}

public class LocalStockReceiptItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReceiptId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? LocationId { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? UnitPrice { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public int? NumberOfBags { get; set; }
    public string PackagingType { get; set; } = "Pallet";
    public string? QrScannedData { get; set; }
    public string? Notes { get; set; }

    public LocalStockReceipt Receipt { get; set; } = null!;
}

public class LocalStockIssue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LocalNumber { get; set; } = string.Empty;
    public string? OfficialNumber { get; set; }
    public string WarehouseId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string IssueDate { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public string? RoNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverIdCard { get; set; }
    public string? VehicleNumber { get; set; }
    public string? TrailerNumber { get; set; }
    public string? IssueReason { get; set; }
    public string? DeliveryTerm { get; set; }
    public string? ContainerNo { get; set; }
    public string? SealNo { get; set; }
    public string? BookingNo { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public string SyncStatus { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsReadOnly { get; set; } = false;

    public ICollection<LocalStockIssueItem> Items { get; set; } = new List<LocalStockIssueItem>();
}

public class LocalStockIssueItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IssueId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? LocationId { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? UnitPrice { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public int? NumberOfBags { get; set; }
    public string PackagingType { get; set; } = "Pallet";
    public string? QrScannedData { get; set; }
    public string? Notes { get; set; }

    public LocalStockIssue Issue { get; set; } = null!;
}

// ============================================================
// LOCAL TRANSFERS
// ============================================================

public class LocalTransfer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LocalNumber { get; set; } = string.Empty;
    public string? OfficialNumber { get; set; }
    public string SourceWarehouseId { get; set; } = string.Empty;
    public string DestWarehouseId { get; set; } = string.Empty;
    public string TransferType { get; set; } = "Logical"; // Logical | Physical
    public string TransferDate { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public string SyncStatus { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsReadOnly { get; set; } = false;

    public ICollection<LocalTransferItem> Items { get; set; } = new List<LocalTransferItem>();
}

public class LocalTransferItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TransferId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? SourceLocationId { get; set; }
    public string? DestLocationId { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }

    public LocalTransfer Transfer { get; set; } = null!;
}

// ============================================================
// LOCAL ATTACHMENTS
// ============================================================

public class LocalAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string LocalFilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string SyncStatus { get; set; } = "Pending";
    public bool IsVerifiedOnServer { get; set; } = false;
    public string? ServerStoragePath { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

// ============================================================
// SYNC MANAGEMENT
// ============================================================

public class SyncLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SyncType { get; set; } = string.Empty;
    public string StartedAt { get; set; } = string.Empty;
    public string? CompletedAt { get; set; }
    public string Status { get; set; } = "InProgress";
    public int ItemsUploaded { get; set; }
    public int ItemsDownloaded { get; set; }
    public int ItemsRejected { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SyncMetadata
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
