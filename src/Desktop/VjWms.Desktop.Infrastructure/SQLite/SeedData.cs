using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;

namespace VjWms.Desktop.Infrastructure.SQLite;

/// <summary>
/// Seeds realistic VJCHEM master data into the local SQLite database.
/// This data simulates what would normally be downloaded from the central server.
/// </summary>
public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedWarehouses(modelBuilder);
        SeedProducts(modelBuilder);
        SeedSuppliers(modelBuilder);
        SeedCustomers(modelBuilder);
        SeedUsers(modelBuilder);
        SeedInventory(modelBuilder);
    }

    private static void SeedWarehouses(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        modelBuilder.Entity<CachedWarehouse>().HasData(
            new CachedWarehouse { Id = "wh-001", Code = "KNĐ", Name = "Kho Nghi Đồng (Nội địa Nghi Sơn)", Type = "Physical", Category = "NoiDiaNS", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-002", Code = "KNĐK", Name = "Kho Nội Địa Khác", Type = "Physical", Category = "NoiDiaKhac", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-003", Code = "KNK", Name = "Kho Nhập Khẩu", Type = "Virtual", Category = "NhapKhau", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-004", Code = "KXK", Name = "Kho Xuất Khẩu", Type = "Virtual", Category = "XuatKhau", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-005", Code = "KCK", Name = "Kho Chuyển Khẩu", Type = "Virtual", Category = "ChuyenKhau", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-006", Code = "KCCDC", Name = "Kho Công Cụ Dụng Cụ", Type = "Physical", Category = "CCDC", IsActive = true, LastSyncedAt = now },
            new CachedWarehouse { Id = "wh-007", Code = "KLOG", Name = "Kho Logistics", Type = "Physical", Category = "Logistics", IsActive = true, LastSyncedAt = now }
        );
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        modelBuilder.Entity<CachedProduct>().HasData(
            new CachedProduct { Id = "prd-001", ProductCode = "CS-50", ProductName = "Caustic Soda Flakes 98%", Unit = "KG", Manufacturer = "Formosa", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-002", ProductCode = "CS-LIQ", ProductName = "Caustic Soda Liquid 50%", Unit = "KG", Manufacturer = "Tokuyama", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-003", ProductCode = "HCL-32", ProductName = "Hydrochloric Acid 32%", Unit = "KG", Manufacturer = "OCI", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-004", ProductCode = "NAOH-P", ProductName = "Sodium Hydroxide Pearls 99%", Unit = "KG", Manufacturer = "Olin", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-005", ProductCode = "H2SO4", ProductName = "Sulfuric Acid 98%", Unit = "KG", Manufacturer = "Lotte Chemical", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-006", ProductCode = "STPP", ProductName = "Sodium Tripolyphosphate", Unit = "KG", Manufacturer = "Yunnan", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-007", ProductCode = "SLS-P", ProductName = "Sodium Lauryl Sulfate Powder", Unit = "KG", Manufacturer = "BASF", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-008", ProductCode = "LABSA", ProductName = "Linear Alkylbenzene Sulfonic Acid", Unit = "KG", Manufacturer = "ISA", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-009", ProductCode = "CaCl2", ProductName = "Calcium Chloride 74-77%", Unit = "KG", Manufacturer = "Nedmag", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-010", ProductCode = "Na2CO3", ProductName = "Soda Ash Dense 99.2%", Unit = "KG", Manufacturer = "Tata Chemicals", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-011", ProductCode = "NaHCO3", ProductName = "Sodium Bicarbonate (Baking Soda)", Unit = "KG", Manufacturer = "Church & Dwight", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-012", ProductCode = "PAC-30", ProductName = "Poly Aluminium Chloride 30%", Unit = "KG", Manufacturer = "Kemira", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-013", ProductCode = "H2O2-50", ProductName = "Hydrogen Peroxide 50%", Unit = "KG", Manufacturer = "Evonik", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-014", ProductCode = "EDTA-4", ProductName = "EDTA Tetrasodium 39%", Unit = "KG", Manufacturer = "Dow", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-015", ProductCode = "CA-STE", ProductName = "Calcium Stearate", Unit = "KG", Manufacturer = "Baerlocher", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-016", ProductCode = "ZN-STE", ProductName = "Zinc Stearate", Unit = "KG", Manufacturer = "PMC Biogenix", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-017", ProductCode = "DOP", ProductName = "Dioctyl Phthalate (DOP)", Unit = "KG", Manufacturer = "LG Chem", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-018", ProductCode = "TIO2-R", ProductName = "Titanium Dioxide Rutile R-902+", Unit = "KG", Manufacturer = "Chemours", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-019", ProductCode = "PE-WAX", ProductName = "Polyethylene Wax", Unit = "KG", Manufacturer = "BASF", IsActive = true, LastSyncedAt = now },
            new CachedProduct { Id = "prd-020", ProductCode = "CPE-135", ProductName = "Chlorinated Polyethylene CPE 135A", Unit = "KG", Manufacturer = "Weifang Yaxing", IsActive = true, LastSyncedAt = now }
        );
    }

    private static void SeedSuppliers(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        modelBuilder.Entity<CachedSupplier>().HasData(
            new CachedSupplier { Id = "sup-001", Code = "NCC-FMS", Name = "Formosa Plastics Corporation", LastSyncedAt = now },
            new CachedSupplier { Id = "sup-002", Code = "NCC-TKY", Name = "Tokuyama Corporation", LastSyncedAt = now },
            new CachedSupplier { Id = "sup-003", Code = "NCC-OCI", Name = "OCI Company Ltd.", LastSyncedAt = now },
            new CachedSupplier { Id = "sup-004", Code = "NCC-BASF", Name = "BASF SE", LastSyncedAt = now },
            new CachedSupplier { Id = "sup-005", Code = "NCC-TATA", Name = "Tata Chemicals Ltd.", LastSyncedAt = now }
        );
    }

    private static void SeedCustomers(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        modelBuilder.Entity<CachedCustomer>().HasData(
            new CachedCustomer { Id = "cus-001", Code = "KH-VNM", Name = "Công ty CP Đường Việt Nam", LastSyncedAt = now },
            new CachedCustomer { Id = "cus-002", Code = "KH-HPG", Name = "Tập đoàn Hòa Phát", LastSyncedAt = now },
            new CachedCustomer { Id = "cus-003", Code = "KH-MSN", Name = "Tập đoàn Masan", LastSyncedAt = now },
            new CachedCustomer { Id = "cus-004", Code = "KH-BMP", Name = "Công ty CP Nhựa Bình Minh", LastSyncedAt = now },
            new CachedCustomer { Id = "cus-005", Code = "KH-DAP", Name = "Công ty CP DAP - Vinachem", LastSyncedAt = now }
        );
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        // Password hashes generated via BCrypt for offline login
        // admin123 -> hash, thukho123 -> hash
        modelBuilder.Entity<CachedUser>().HasData(
            new CachedUser
            {
                Id = "usr-001", Username = "admin", FullName = "Quản trị viên",
                PasswordHash = "admin123", // Phase 1: plain text comparison. Phase 2+: BCrypt
                Role = "Admin", Permissions = "[\"all\"]",
                PreferredLanguage = "vi-VN", LastSyncedAt = now
            },
            new CachedUser
            {
                Id = "usr-002", Username = "thukho", FullName = "Thủ Kho Nghi Sơn",
                PasswordHash = "thukho123",
                Role = "WarehouseKeeper", Permissions = "[\"receipt.create\",\"issue.create\",\"inventory.view\"]",
                PreferredLanguage = "vi-VN", LastSyncedAt = now
            }
        );
    }

    private static void SeedInventory(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow.ToString("o");
        // Seed initial inventory snapshot for Kho Nghi Đồng
        modelBuilder.Entity<CachedInventory>().HasData(
            new CachedInventory { Id = "inv-001", WarehouseId = "wh-001", ProductId = "prd-001", Quantity = 25000, SnapshotAt = now },
            new CachedInventory { Id = "inv-002", WarehouseId = "wh-001", ProductId = "prd-002", Quantity = 18000, SnapshotAt = now },
            new CachedInventory { Id = "inv-003", WarehouseId = "wh-001", ProductId = "prd-003", Quantity = 12000, SnapshotAt = now },
            new CachedInventory { Id = "inv-004", WarehouseId = "wh-001", ProductId = "prd-005", Quantity = 8000, SnapshotAt = now },
            new CachedInventory { Id = "inv-005", WarehouseId = "wh-001", ProductId = "prd-010", Quantity = 30000, SnapshotAt = now },
            new CachedInventory { Id = "inv-006", WarehouseId = "wh-003", ProductId = "prd-006", Quantity = 15000, SnapshotAt = now },
            new CachedInventory { Id = "inv-007", WarehouseId = "wh-003", ProductId = "prd-007", Quantity = 5000, SnapshotAt = now },
            new CachedInventory { Id = "inv-008", WarehouseId = "wh-004", ProductId = "prd-001", Quantity = 40000, SnapshotAt = now },
            new CachedInventory { Id = "inv-009", WarehouseId = "wh-004", ProductId = "prd-008", Quantity = 10000, SnapshotAt = now },
            new CachedInventory { Id = "inv-010", WarehouseId = "wh-002", ProductId = "prd-012", Quantity = 7000, SnapshotAt = now }
        );
    }
}
