using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuditFramework.ApiService.Data;

public class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=auditdb;Username=postgres;Password=postgres",
            o => o.MigrationsHistoryTable("__ef_migrations", "public"));
        return new AuditDbContext(optionsBuilder.Options);
    }
}
