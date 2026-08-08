using VjWms.Shared.Enums;

namespace VjWms.Server.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public WarehouseType Type { get; set; }
    public WarehouseCategory Category { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
}

public class WarehouseLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehouseId { get; set; }
    public string? Zone { get; set; }
    public string? Row { get; set; }
    public string? Shelf { get; set; }
    public string? Bin { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Supplier
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Contact { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Contact { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PreferredLanguage { get; set; } = "vi-VN";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class StockReceipt
{
    public Guid Id { get; set; }
    public string? DocumentNumber { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public string? ContractNumber { get; set; }
    public string? RoNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverIdCard { get; set; }
    public string? VehicleNumber { get; set; }
    public string? TrailerNumber { get; set; }
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Synced;
    public string? SourceClientId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; } = 1;

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<StockReceiptItem> Items { get; set; } = new List<StockReceiptItem>();
}

public class StockReceiptItem
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public int? NumberOfBags { get; set; }
    public PackagingType PackagingType { get; set; } = PackagingType.Pallet;
    public string? QrScannedData { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public StockReceipt Receipt { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class StockIssue
{
    public Guid Id { get; set; }
    public string? DocumentNumber { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateOnly IssueDate { get; set; }
    public string? ContractNumber { get; set; }
    public string? RoNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverIdCard { get; set; }
    public string? VehicleNumber { get; set; }
    public string? TrailerNumber { get; set; }
    public string? IssueReason { get; set; }
    public string? IssueLocation { get; set; }
    public DeliveryTerm? DeliveryTerm { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? ContainerNo { get; set; }
    public string? SealNo { get; set; }
    public string? BookingNo { get; set; }
    public string? Etd { get; set; }
    public string? Pol { get; set; }
    public string? Pod { get; set; }
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Synced;
    public string? SourceClientId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; } = 1;

    // Navigation
    public Warehouse Warehouse { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<StockIssueItem> Items { get; set; } = new List<StockIssueItem>();
}

public class StockIssueItem
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public int? NumberOfBags { get; set; }
    public PackagingType PackagingType { get; set; } = PackagingType.Pallet;
    public string? QrScannedData { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public StockIssue Issue { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class InventoryTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; set; }
}

public class DocumentSequence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DocumentType DocumentType { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int LastSequence { get; set; } = 0;
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? ClientId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
