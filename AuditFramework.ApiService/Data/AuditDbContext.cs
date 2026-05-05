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
            e.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
            e.Property(u => u.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
            e.Property(u => u.Bio).HasColumnName("bio");
            e.Property(u => u.Role).HasColumnName("role").HasMaxLength(50).HasDefaultValue("user");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.ToTable("events", "audit");
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(ev => ev.ActorId).HasColumnName("actor_id");
            e.Property(ev => ev.Action).HasColumnName("action").IsRequired();
            e.Property(ev => ev.ResourceType).HasColumnName("resource_type").HasDefaultValue("user");
            e.Property(ev => ev.ResourceId).HasColumnName("resource_id");
            e.Property(ev => ev.OldResource).HasColumnName("old_resource").HasColumnType("jsonb");
            e.Property(ev => ev.NewResource).HasColumnName("new_resource").HasColumnType("jsonb");
            e.Property(ev => ev.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            e.Property(ev => ev.OccurredAt).HasColumnName("occurred_at");
            e.HasIndex(ev => new { ev.ResourceId, ev.OccurredAt })
                .HasDatabaseName("idx_audit_events_resource");
            e.HasIndex(ev => ev.ActorId)
                .HasDatabaseName("idx_audit_events_actor");
        });
    }
}
