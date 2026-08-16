using Microsoft.EntityFrameworkCore;
using VjWms.Desktop.Domain.Entities;

namespace VjWms.Desktop.Infrastructure.SQLite;

/// <summary>
/// Per-user SQLite database context.
/// Each user gets their own database at: %APPDATA%/vj-wms/users/{userId}/local.db
/// </summary>
public class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

    // Cached Master Data
    public DbSet<CachedWarehouse> CachedWarehouses => Set<CachedWarehouse>();
    public DbSet<CachedProduct> CachedProducts => Set<CachedProduct>();
    public DbSet<CachedSupplier> CachedSuppliers => Set<CachedSupplier>();
    public DbSet<CachedCustomer> CachedCustomers => Set<CachedCustomer>();
    public DbSet<CachedUser> CachedUsers => Set<CachedUser>();
    public DbSet<CachedInventory> CachedInventories => Set<CachedInventory>();

    // Local Transactions
    public DbSet<LocalStockReceipt> LocalStockReceipts => Set<LocalStockReceipt>();
    public DbSet<LocalStockReceiptItem> LocalStockReceiptItems => Set<LocalStockReceiptItem>();
    public DbSet<LocalStockIssue> LocalStockIssues => Set<LocalStockIssue>();
    public DbSet<LocalStockIssueItem> LocalStockIssueItems => Set<LocalStockIssueItem>();
    public DbSet<LocalTransfer> LocalTransfers => Set<LocalTransfer>();
    public DbSet<LocalTransferItem> LocalTransferItems => Set<LocalTransferItem>();

    // Attachments
    public DbSet<LocalAttachment> LocalAttachments => Set<LocalAttachment>();

    // Sync
    public DbSet<SyncLog> SyncLogs => Set<SyncLog>();
    public DbSet<SyncMetadata> SyncMetadata => Set<SyncMetadata>();

    // Edit History
    public DbSet<LocalEditHistory> EditHistories => Set<LocalEditHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === Cached Master Data ===
        modelBuilder.Entity<CachedWarehouse>(e => { e.HasKey(w => w.Id); });
        modelBuilder.Entity<CachedProduct>(e => { e.HasKey(p => p.Id); });
        modelBuilder.Entity<CachedSupplier>(e => { e.HasKey(s => s.Id); });
        modelBuilder.Entity<CachedCustomer>(e => { e.HasKey(c => c.Id); });
        modelBuilder.Entity<CachedUser>(e => { e.HasKey(u => u.Id); });
        modelBuilder.Entity<CachedInventory>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.WarehouseId);
            e.HasIndex(i => i.ProductId);
        });

        // === Local Stock Receipts ===
        modelBuilder.Entity<LocalStockReceipt>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.SyncStatus);
            e.HasIndex(r => r.Status);
        });

        modelBuilder.Entity<LocalStockReceiptItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasOne(i => i.Receipt).WithMany(r => r.Items).HasForeignKey(i => i.ReceiptId);
        });

        // === Local Stock Issues ===
        modelBuilder.Entity<LocalStockIssue>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.SyncStatus);
            e.HasIndex(i => i.Status);
        });

        modelBuilder.Entity<LocalStockIssueItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasOne(i => i.Issue).WithMany(r => r.Items).HasForeignKey(i => i.IssueId);
        });

        // === Local Transfers ===
        modelBuilder.Entity<LocalTransfer>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.SyncStatus);
            e.HasIndex(t => t.Status);
        });

        modelBuilder.Entity<LocalTransferItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasOne(i => i.Transfer).WithMany(t => t.Items).HasForeignKey(i => i.TransferId);
        });

        // === Attachments ===
        modelBuilder.Entity<LocalAttachment>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.SyncStatus);
        });

        // === Sync ===
        modelBuilder.Entity<SyncLog>(e => { e.HasKey(s => s.Id); });
        modelBuilder.Entity<SyncMetadata>(e => { e.HasKey(s => s.Key); });

        // === Seed Data ===
        SeedData.Seed(modelBuilder);
    }
}
