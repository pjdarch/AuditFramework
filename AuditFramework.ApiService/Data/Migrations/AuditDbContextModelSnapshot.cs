using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AuditFramework.ApiService.Data;

#nullable disable

namespace AuditFramework.ApiService.Data.Migrations;

[DbContext(typeof(AuditDbContext))]
partial class AuditDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("AuditFramework.ApiService.Data.Models.AuditEvent", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<Guid>("ActorId").HasColumnType("uuid").HasColumnName("actor_id");
            b.Property<string>("Action").IsRequired().HasColumnType("text").HasColumnName("action");
            b.Property<string>("ResourceType").IsRequired().HasColumnType("text").HasDefaultValue("user").HasColumnName("resource_type");
            b.Property<Guid>("ResourceId").HasColumnType("uuid").HasColumnName("resource_id");
            b.Property<string>("OldResource").HasColumnType("jsonb").HasColumnName("old_resource");
            b.Property<string>("NewResource").IsRequired().HasColumnType("jsonb").HasColumnName("new_resource");
            b.Property<string>("Metadata").HasColumnType("jsonb").HasColumnName("metadata");
            b.Property<DateTimeOffset>("OccurredAt").HasColumnType("timestamp with time zone").HasColumnName("occurred_at");
            b.HasKey("Id");
            b.ToTable("events", "audit");
        });

        modelBuilder.Entity("AuditFramework.ApiService.Data.Models.User", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)").HasColumnName("email");
            b.Property<string>("Name").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)").HasColumnName("name");
            b.Property<string>("Bio").HasColumnType("text").HasColumnName("bio");
            b.Property<string>("Role").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)").HasDefaultValue("user").HasColumnName("role");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone").HasColumnName("created_at");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("users", "public");
        });
#pragma warning restore 612, 618
    }
}
