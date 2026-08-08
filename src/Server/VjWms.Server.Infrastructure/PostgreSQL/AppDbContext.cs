using Microsoft.EntityFrameworkCore;
using VjWms.Server.Domain.Entities;
using VjWms.Shared.Enums;

namespace VjWms.Server.Infrastructure.PostgreSQL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Core
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();

    // Users & Permissions
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Documents
    public DbSet<StockReceipt> StockReceipts => Set<StockReceipt>();
    public DbSet<StockReceiptItem> StockReceiptItems => Set<StockReceiptItem>();
    public DbSet<StockIssue> StockIssues => Set<StockIssue>();
    public DbSet<StockIssueItem> StockIssueItems => Set<StockIssueItem>();

    // Inventory
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // System
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === Warehouse ===
        modelBuilder.Entity<Warehouse>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.Code).IsUnique();
            e.Property(w => w.Code).HasMaxLength(20).IsRequired();
            e.Property(w => w.Name).HasMaxLength(200).IsRequired();
            e.Property(w => w.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(w => w.Category).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<WarehouseLocation>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Warehouse).WithMany(w => w.Locations).HasForeignKey(l => l.WarehouseId);
        });

        // === Product ===
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.ProductCode).IsUnique();
            e.Property(p => p.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(p => p.ProductName).HasMaxLength(500).IsRequired();
            e.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        });

        // === Supplier ===
        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Code).IsUnique();
            e.Property(s => s.Code).HasMaxLength(30).IsRequired();
            e.Property(s => s.Name).HasMaxLength(500).IsRequired();
        });

        // === Customer ===
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(30).IsRequired();
            e.Property(c => c.Name).HasMaxLength(500).IsRequired();
        });

        // === User / Role / Permission ===
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("vi-VN");
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Name).IsUnique();
            e.Property(r => r.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Code).IsUnique();
            e.Property(p => p.Code).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            e.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            e.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        // === StockReceipt ===
        modelBuilder.Entity<StockReceipt>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.DocumentNumber).IsUnique().HasFilter("\"DocumentNumber\" IS NOT NULL");
            e.Property(r => r.DocumentNumber).HasMaxLength(50);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);
            e.HasOne(r => r.Warehouse).WithMany().HasForeignKey(r => r.WarehouseId);
            e.HasOne(r => r.Supplier).WithMany().HasForeignKey(r => r.SupplierId);
        });

        modelBuilder.Entity<StockReceiptItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Quantity).HasPrecision(18, 4);
            e.Property(i => i.UnitPrice).HasPrecision(18, 4);
            e.Property(i => i.PackagingType).HasConversion<string>().HasMaxLength(20);
            e.HasOne(i => i.Receipt).WithMany(r => r.Items).HasForeignKey(i => i.ReceiptId);
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
        });

        // === StockIssue ===
        modelBuilder.Entity<StockIssue>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.DocumentNumber).IsUnique().HasFilter("\"DocumentNumber\" IS NOT NULL");
            e.Property(i => i.DocumentNumber).HasMaxLength(50);
            e.Property(i => i.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(i => i.DeliveryTerm).HasConversion<string?>().HasMaxLength(10);
            e.HasOne(i => i.Warehouse).WithMany().HasForeignKey(i => i.WarehouseId);
            e.HasOne(i => i.Customer).WithMany().HasForeignKey(i => i.CustomerId);
        });

        modelBuilder.Entity<StockIssueItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Quantity).HasPrecision(18, 4);
            e.Property(i => i.UnitPrice).HasPrecision(18, 4);
            e.Property(i => i.PackagingType).HasConversion<string>().HasMaxLength(20);
            e.HasOne(i => i.Issue).WithMany(r => r.Items).HasForeignKey(i => i.IssueId);
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
        });

        // === Inventory Ledger ===
        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Quantity).HasPrecision(18, 4);
            e.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.ReferenceType).HasMaxLength(30);
            e.HasIndex(t => t.WarehouseId);
            e.HasIndex(t => t.ProductId);
            e.HasIndex(t => t.TransactionDate);
            e.HasIndex(t => new { t.ReferenceId, t.ReferenceType });
        });

        // === Document Sequence ===
        modelBuilder.Entity<DocumentSequence>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.DocumentType).HasConversion<string>().HasMaxLength(30);
            e.Property(s => s.WarehouseCode).HasMaxLength(20);
            e.HasIndex(s => new { s.DocumentType, s.WarehouseCode, s.Date }).IsUnique();
        });

        // === Audit Log ===
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(50);
            e.Property(a => a.EntityType).HasMaxLength(50);
            e.HasIndex(a => new { a.EntityType, a.EntityId });
            e.HasIndex(a => a.CreatedAt);
        });

        // === Seed 7 Warehouses ===
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Code = "KHO-NDNS", Name = "Kho hàng nội địa hàng mua Nghi Sơn", Type = WarehouseType.Physical, Category = WarehouseCategory.NoiDiaNghiSon },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Code = "KHO-NDK", Name = "Kho hàng nội địa khác", Type = WarehouseType.Physical, Category = WarehouseCategory.NoiDiaKhac },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Code = "KHO-NK", Name = "Kho hàng nhập khẩu", Type = WarehouseType.Virtual, Category = WarehouseCategory.NhapKhau },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Code = "KHO-XK", Name = "Kho hàng xuất khẩu", Type = WarehouseType.Virtual, Category = WarehouseCategory.XuatKhau },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Code = "KHO-CK", Name = "Kho hàng chuyển khẩu (Ngoài lãnh thổ VN)", Type = WarehouseType.Virtual, Category = WarehouseCategory.ChuyenKhau },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Code = "KHO-CCDC", Name = "Kho công cụ dụng cụ", Type = WarehouseType.Physical, Category = WarehouseCategory.CongCuDungCu },
            new Warehouse { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Code = "KHO-LOG", Name = "Kho làm dịch vụ logistics", Type = WarehouseType.Physical, Category = WarehouseCategory.Logistics }
        );

        // Seed roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Admin", Description = "System Administrator" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "WarehouseStaff", Description = "Nhân viên kho" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "WarehouseManager", Description = "Thủ kho" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), Name = "Accountant", Description = "Kế toán" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000005"), Name = "ChiefAccountant", Description = "Kế toán trưởng (KTT)" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000006"), Name = "SalesStaff", Description = "Nhân viên Sales" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000007"), Name = "PurchasingStaff", Description = "Nhân viên Mua hàng" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000008"), Name = "LogisticsStaff", Description = "Nhân viên Logistics" },
            new Role { Id = Guid.Parse("20000000-0000-0000-0000-000000000009"), Name = "Director", Description = "Giám đốc" }
        );

        // Seed admin user (password: admin123)
        var adminId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator",
                Email = "admin@vjchem.vn",
                PreferredLanguage = "vi-VN",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = adminId, RoleId = Guid.Parse("20000000-0000-0000-0000-000000000001") }
        );
    }
}
