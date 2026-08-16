using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VjWms.Desktop.Infrastructure.SQLite;

public class LocalDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
{
    public LocalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocalDbContext>();
        optionsBuilder.UseSqlite("Data Source=vjwms.db");

        return new LocalDbContext(optionsBuilder.Options);
    }
}
