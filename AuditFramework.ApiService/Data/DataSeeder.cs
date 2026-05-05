using AuditFramework.ApiService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditFramework.ApiService.Data;

public static class DataSeeder
{
    public static readonly Guid AdminId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid UserId  = new("00000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AuditDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        db.Users.AddRange(
            new User
            {
                Id = AdminId,
                Email = "admin@example.com",
                Name = "Admin User",
                Bio = "System administrator",
                Role = "admin",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = UserId,
                Email = "user@example.com",
                Name = "Regular User",
                Bio = "Just a regular user",
                Role = "user",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync();
    }
}
