using AuditFramework.ApiService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditFramework.ApiService.Data;

public static class DataSeeder
{
    public static readonly Guid AdminId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid UserId  = new("00000000-0000-0000-0000-000000000002");

    private static readonly User[] SeedUsers =
    [
        new() { Id = AdminId,
                Email = "admin@example.com", Name = "Admin User",
                Bio = "System administrator with full access.", Role = "admin" },
        new() { Id = UserId,
                Email = "user@example.com", Name = "Regular User",
                Bio = "Standard user account.", Role = "user" },
        new() { Id = new Guid("00000000-0000-0000-0000-000000000003"),
                Email = "alice@example.com", Name = "Alice Santos",
                Bio = "Product designer focused on user experience.", Role = "user" },
        new() { Id = new Guid("00000000-0000-0000-0000-000000000004"),
                Email = "bob@example.com", Name = "Bob Ferreira",
                Bio = "Backend engineer working on platform services.", Role = "user" },
        new() { Id = new Guid("00000000-0000-0000-0000-000000000005"),
                Email = "carol@example.com", Name = "Carol Lima",
                Bio = "Data analyst specialising in growth metrics.", Role = "user" },
        new() { Id = new Guid("00000000-0000-0000-0000-000000000006"),
                Email = "dave@example.com", Name = "Dave Moreira",
                Bio = "DevOps engineer managing cloud infrastructure.", Role = "user" },
        new() { Id = new Guid("00000000-0000-0000-0000-000000000007"),
                Email = "eve@example.com", Name = "Eve Costa",
                Bio = "Security researcher and compliance lead.", Role = "user" },
    ];

    public static async Task SeedAsync(AuditDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;
        foreach (var u in SeedUsers)
        {
            u.CreatedAt = now;
            u.UpdatedAt = now;
        }

        db.Users.AddRange(SeedUsers);
        await db.SaveChangesAsync();
    }

    public static string[] AllEmails => SeedUsers.Select(u => u.Email).ToArray();
}
