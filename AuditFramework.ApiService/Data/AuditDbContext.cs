using AuditFramework.ApiService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditFramework.ApiService.Data;

public class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditEvent> Events => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users", "public");
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Id).ValueGeneratedNever();
            e.Property(u => u.Role).HasDefaultValue("user");
        });

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.ToTable("events", "audit");
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Id).ValueGeneratedNever();
            e.Property(ev => ev.ResourceType).HasDefaultValue("user");
            e.Property(ev => ev.OldResource).HasColumnType("jsonb");
            e.Property(ev => ev.NewResource).HasColumnType("jsonb");
            e.Property(ev => ev.Metadata).HasColumnType("jsonb");
            e.HasIndex(ev => new { ev.ResourceId, ev.OccurredAt })
                .HasDatabaseName("idx_audit_events_resource");
            e.HasIndex(ev => ev.ActorId)
                .HasDatabaseName("idx_audit_events_actor");
        });
    }
}
