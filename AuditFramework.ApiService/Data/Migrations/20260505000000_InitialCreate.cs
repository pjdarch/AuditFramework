using AuditFramework.ApiService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditFramework.ApiService.Data.Migrations;

[DbContext(typeof(AuditDbContext))]
[Migration("20260505000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "audit");

        migrationBuilder.CreateTable(
            name: "users",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                bio = table.Column<string>(type: "text", nullable: true),
                role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "user"),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "events",
            schema: "audit",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                resource_type = table.Column<string>(type: "text", nullable: false, defaultValue: "user"),
                resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                old_resource = table.Column<string>(type: "jsonb", nullable: true),
                new_resource = table.Column<string>(type: "jsonb", nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_events", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_users_email",
            schema: "public",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "idx_audit_events_actor",
            schema: "audit",
            table: "events",
            column: "actor_id");

        migrationBuilder.CreateIndex(
            name: "idx_audit_events_resource",
            schema: "audit",
            table: "events",
            columns: new[] { "resource_id", "occurred_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "events", schema: "audit");
        migrationBuilder.DropTable(name: "users", schema: "public");
    }
}
