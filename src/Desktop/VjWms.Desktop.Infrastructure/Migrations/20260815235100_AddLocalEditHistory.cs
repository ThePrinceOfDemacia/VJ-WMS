using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VjWms.Desktop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalEditHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedCustomers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastSyncedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedInventories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    WarehouseId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    LocationId = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    ReservedQuantity = table.Column<double>(type: "REAL", nullable: false),
                    SnapshotAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedProducts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", nullable: true),
                    LotNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSyncedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedSuppliers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastSyncedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedSuppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    LastSyncedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedWarehouses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSyncedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedWarehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentId = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedAt = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalAttachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentId = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    LocalFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    SyncStatus = table.Column<string>(type: "TEXT", nullable: false),
                    IsVerifiedOnServer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerStoragePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalStockIssues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LocalNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OfficialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    WarehouseId = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<string>(type: "TEXT", nullable: true),
                    IssueDate = table.Column<string>(type: "TEXT", nullable: false),
                    ContractNumber = table.Column<string>(type: "TEXT", nullable: true),
                    RoNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DriverName = table.Column<string>(type: "TEXT", nullable: true),
                    DriverIdCard = table.Column<string>(type: "TEXT", nullable: true),
                    VehicleNumber = table.Column<string>(type: "TEXT", nullable: true),
                    TrailerNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IssueReason = table.Column<string>(type: "TEXT", nullable: true),
                    DeliveryTerm = table.Column<string>(type: "TEXT", nullable: true),
                    ContainerNo = table.Column<string>(type: "TEXT", nullable: true),
                    SealNo = table.Column<string>(type: "TEXT", nullable: true),
                    BookingNo = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<string>(type: "TEXT", nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalStockIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalStockReceipts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LocalNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OfficialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    WarehouseId = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierId = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiptDate = table.Column<string>(type: "TEXT", nullable: false),
                    ContractNumber = table.Column<string>(type: "TEXT", nullable: true),
                    RoNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DriverName = table.Column<string>(type: "TEXT", nullable: true),
                    DriverIdCard = table.Column<string>(type: "TEXT", nullable: true),
                    VehicleNumber = table.Column<string>(type: "TEXT", nullable: true),
                    TrailerNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<string>(type: "TEXT", nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalStockReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalTransfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LocalNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OfficialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SourceWarehouseId = table.Column<string>(type: "TEXT", nullable: false),
                    DestWarehouseId = table.Column<string>(type: "TEXT", nullable: false),
                    TransferType = table.Column<string>(type: "TEXT", nullable: false),
                    TransferDate = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SyncStatus = table.Column<string>(type: "TEXT", nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalTransfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SyncType = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ItemsUploaded = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemsDownloaded = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemsRejected = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncMetadata",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncMetadata", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "LocalStockIssueItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IssueId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    LocationId = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<double>(type: "REAL", nullable: true),
                    LotNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: true),
                    NumberOfBags = table.Column<int>(type: "INTEGER", nullable: true),
                    PackagingType = table.Column<string>(type: "TEXT", nullable: false),
                    QrScannedData = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalStockIssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalStockIssueItems_LocalStockIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "LocalStockIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalStockReceiptItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ReceiptId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    LocationId = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<double>(type: "REAL", nullable: true),
                    LotNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: true),
                    NumberOfBags = table.Column<int>(type: "INTEGER", nullable: true),
                    PackagingType = table.Column<string>(type: "TEXT", nullable: false),
                    QrScannedData = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalStockReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalStockReceiptItems_LocalStockReceipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "LocalStockReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalTransferItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TransferId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceLocationId = table.Column<string>(type: "TEXT", nullable: true),
                    DestLocationId = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    LotNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalTransferItems_LocalTransfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "LocalTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CachedCustomers",
                columns: new[] { "Id", "Code", "LastSyncedAt", "Name" },
                values: new object[,]
                {
                    { "cus-001", "KH-VNM", "2026-08-15T23:51:00.1867919Z", "Công ty CP Đường Việt Nam" },
                    { "cus-002", "KH-HPG", "2026-08-15T23:51:00.1867919Z", "Tập đoàn Hòa Phát" },
                    { "cus-003", "KH-MSN", "2026-08-15T23:51:00.1867919Z", "Tập đoàn Masan" },
                    { "cus-004", "KH-BMP", "2026-08-15T23:51:00.1867919Z", "Công ty CP Nhựa Bình Minh" },
                    { "cus-005", "KH-DAP", "2026-08-15T23:51:00.1867919Z", "Công ty CP DAP - Vinachem" }
                });

            migrationBuilder.InsertData(
                table: "CachedInventories",
                columns: new[] { "Id", "LocationId", "ProductId", "Quantity", "ReservedQuantity", "SnapshotAt", "WarehouseId" },
                values: new object[,]
                {
                    { "inv-001", null, "prd-001", 25000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-001" },
                    { "inv-002", null, "prd-002", 18000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-001" },
                    { "inv-003", null, "prd-003", 12000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-001" },
                    { "inv-004", null, "prd-005", 8000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-001" },
                    { "inv-005", null, "prd-010", 30000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-001" },
                    { "inv-006", null, "prd-006", 15000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-003" },
                    { "inv-007", null, "prd-007", 5000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-003" },
                    { "inv-008", null, "prd-001", 40000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-004" },
                    { "inv-009", null, "prd-008", 10000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-004" },
                    { "inv-010", null, "prd-012", 7000.0, 0.0, "2026-08-15T23:51:00.1872543Z", "wh-002" }
                });

            migrationBuilder.InsertData(
                table: "CachedProducts",
                columns: new[] { "Id", "BatchNumber", "IsActive", "LastSyncedAt", "LotNumber", "Manufacturer", "ProductCode", "ProductName", "Unit" },
                values: new object[,]
                {
                    { "prd-001", null, true, "2026-08-15T23:51:00.1863400Z", null, "Formosa", "CS-50", "Caustic Soda Flakes 98%", "KG" },
                    { "prd-002", null, true, "2026-08-15T23:51:00.1863400Z", null, "Tokuyama", "CS-LIQ", "Caustic Soda Liquid 50%", "KG" },
                    { "prd-003", null, true, "2026-08-15T23:51:00.1863400Z", null, "OCI", "HCL-32", "Hydrochloric Acid 32%", "KG" },
                    { "prd-004", null, true, "2026-08-15T23:51:00.1863400Z", null, "Olin", "NAOH-P", "Sodium Hydroxide Pearls 99%", "KG" },
                    { "prd-005", null, true, "2026-08-15T23:51:00.1863400Z", null, "Lotte Chemical", "H2SO4", "Sulfuric Acid 98%", "KG" },
                    { "prd-006", null, true, "2026-08-15T23:51:00.1863400Z", null, "Yunnan", "STPP", "Sodium Tripolyphosphate", "KG" },
                    { "prd-007", null, true, "2026-08-15T23:51:00.1863400Z", null, "BASF", "SLS-P", "Sodium Lauryl Sulfate Powder", "KG" },
                    { "prd-008", null, true, "2026-08-15T23:51:00.1863400Z", null, "ISA", "LABSA", "Linear Alkylbenzene Sulfonic Acid", "KG" },
                    { "prd-009", null, true, "2026-08-15T23:51:00.1863400Z", null, "Nedmag", "CaCl2", "Calcium Chloride 74-77%", "KG" },
                    { "prd-010", null, true, "2026-08-15T23:51:00.1863400Z", null, "Tata Chemicals", "Na2CO3", "Soda Ash Dense 99.2%", "KG" },
                    { "prd-011", null, true, "2026-08-15T23:51:00.1863400Z", null, "Church & Dwight", "NaHCO3", "Sodium Bicarbonate (Baking Soda)", "KG" },
                    { "prd-012", null, true, "2026-08-15T23:51:00.1863400Z", null, "Kemira", "PAC-30", "Poly Aluminium Chloride 30%", "KG" },
                    { "prd-013", null, true, "2026-08-15T23:51:00.1863400Z", null, "Evonik", "H2O2-50", "Hydrogen Peroxide 50%", "KG" },
                    { "prd-014", null, true, "2026-08-15T23:51:00.1863400Z", null, "Dow", "EDTA-4", "EDTA Tetrasodium 39%", "KG" },
                    { "prd-015", null, true, "2026-08-15T23:51:00.1863400Z", null, "Baerlocher", "CA-STE", "Calcium Stearate", "KG" },
                    { "prd-016", null, true, "2026-08-15T23:51:00.1863400Z", null, "PMC Biogenix", "ZN-STE", "Zinc Stearate", "KG" },
                    { "prd-017", null, true, "2026-08-15T23:51:00.1863400Z", null, "LG Chem", "DOP", "Dioctyl Phthalate (DOP)", "KG" },
                    { "prd-018", null, true, "2026-08-15T23:51:00.1863400Z", null, "Chemours", "TIO2-R", "Titanium Dioxide Rutile R-902+", "KG" },
                    { "prd-019", null, true, "2026-08-15T23:51:00.1863400Z", null, "BASF", "PE-WAX", "Polyethylene Wax", "KG" },
                    { "prd-020", null, true, "2026-08-15T23:51:00.1863400Z", null, "Weifang Yaxing", "CPE-135", "Chlorinated Polyethylene CPE 135A", "KG" }
                });

            migrationBuilder.InsertData(
                table: "CachedSuppliers",
                columns: new[] { "Id", "Code", "LastSyncedAt", "Name" },
                values: new object[,]
                {
                    { "sup-001", "NCC-FMS", "2026-08-15T23:51:00.1865959Z", "Formosa Plastics Corporation" },
                    { "sup-002", "NCC-TKY", "2026-08-15T23:51:00.1865959Z", "Tokuyama Corporation" },
                    { "sup-003", "NCC-OCI", "2026-08-15T23:51:00.1865959Z", "OCI Company Ltd." },
                    { "sup-004", "NCC-BASF", "2026-08-15T23:51:00.1865959Z", "BASF SE" },
                    { "sup-005", "NCC-TATA", "2026-08-15T23:51:00.1865959Z", "Tata Chemicals Ltd." }
                });

            migrationBuilder.InsertData(
                table: "CachedUsers",
                columns: new[] { "Id", "FullName", "LastSyncedAt", "PasswordHash", "Permissions", "PreferredLanguage", "Role", "Username" },
                values: new object[,]
                {
                    { "usr-001", "Quản trị viên", "2026-08-15T23:51:00.1869634Z", "admin123", "[\"all\"]", "vi-VN", "Admin", "admin" },
                    { "usr-002", "Thủ Kho Nghi Sơn", "2026-08-15T23:51:00.1869634Z", "thukho123", "[\"receipt.create\",\"issue.create\",\"inventory.view\"]", "vi-VN", "WarehouseKeeper", "thukho" }
                });

            migrationBuilder.InsertData(
                table: "CachedWarehouses",
                columns: new[] { "Id", "Category", "Code", "IsActive", "LastSyncedAt", "Name", "Type" },
                values: new object[,]
                {
                    { "wh-001", "NoiDiaNS", "KNĐ", true, "2026-08-15T23:51:00.1851112Z", "Kho Nghi Đồng (Nội địa Nghi Sơn)", "Physical" },
                    { "wh-002", "NoiDiaKhac", "KNĐK", true, "2026-08-15T23:51:00.1851112Z", "Kho Nội Địa Khác", "Physical" },
                    { "wh-003", "NhapKhau", "KNK", true, "2026-08-15T23:51:00.1851112Z", "Kho Nhập Khẩu", "Virtual" },
                    { "wh-004", "XuatKhau", "KXK", true, "2026-08-15T23:51:00.1851112Z", "Kho Xuất Khẩu", "Virtual" },
                    { "wh-005", "ChuyenKhau", "KCK", true, "2026-08-15T23:51:00.1851112Z", "Kho Chuyển Khẩu", "Virtual" },
                    { "wh-006", "CCDC", "KCCDC", true, "2026-08-15T23:51:00.1851112Z", "Kho Công Cụ Dụng Cụ", "Physical" },
                    { "wh-007", "Logistics", "KLOG", true, "2026-08-15T23:51:00.1851112Z", "Kho Logistics", "Physical" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedInventories_ProductId",
                table: "CachedInventories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CachedInventories_WarehouseId",
                table: "CachedInventories",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalAttachments_SyncStatus",
                table: "LocalAttachments",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockIssueItems_IssueId",
                table: "LocalStockIssueItems",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockIssues_Status",
                table: "LocalStockIssues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockIssues_SyncStatus",
                table: "LocalStockIssues",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockReceiptItems_ReceiptId",
                table: "LocalStockReceiptItems",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockReceipts_Status",
                table: "LocalStockReceipts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LocalStockReceipts_SyncStatus",
                table: "LocalStockReceipts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTransferItems_TransferId",
                table: "LocalTransferItems",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTransfers_Status",
                table: "LocalTransfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTransfers_SyncStatus",
                table: "LocalTransfers",
                column: "SyncStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedCustomers");

            migrationBuilder.DropTable(
                name: "CachedInventories");

            migrationBuilder.DropTable(
                name: "CachedProducts");

            migrationBuilder.DropTable(
                name: "CachedSuppliers");

            migrationBuilder.DropTable(
                name: "CachedUsers");

            migrationBuilder.DropTable(
                name: "CachedWarehouses");

            migrationBuilder.DropTable(
                name: "EditHistories");

            migrationBuilder.DropTable(
                name: "LocalAttachments");

            migrationBuilder.DropTable(
                name: "LocalStockIssueItems");

            migrationBuilder.DropTable(
                name: "LocalStockReceiptItems");

            migrationBuilder.DropTable(
                name: "LocalTransferItems");

            migrationBuilder.DropTable(
                name: "SyncLogs");

            migrationBuilder.DropTable(
                name: "SyncMetadata");

            migrationBuilder.DropTable(
                name: "LocalStockIssues");

            migrationBuilder.DropTable(
                name: "LocalStockReceipts");

            migrationBuilder.DropTable(
                name: "LocalTransfers");
        }
    }
}
