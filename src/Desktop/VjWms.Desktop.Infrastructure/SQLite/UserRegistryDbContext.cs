using Microsoft.EntityFrameworkCore;

namespace VjWms.Desktop.Infrastructure.SQLite;

public class RegisteredUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string LastLoginAt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AppSettings
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Shared database for the entire machine.
/// Location: %APPDATA%/vj-wms/user_registry.db
/// </summary>
public class UserRegistryDbContext : DbContext
{
    public UserRegistryDbContext(DbContextOptions<UserRegistryDbContext> options) : base(options) { }

    public DbSet<RegisteredUser> RegisteredUsers => Set<RegisteredUser>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RegisteredUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<AppSettings>(e =>
        {
            e.HasKey(a => a.Key);
        });
    }
}
