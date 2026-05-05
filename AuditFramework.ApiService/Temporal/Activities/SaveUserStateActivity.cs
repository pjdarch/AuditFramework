using AuditFramework.ApiService.Data;
using AuditFramework.ApiService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;

namespace AuditFramework.ApiService.Temporal.Activities;

public record SaveUserStateRequest(
    Guid UserId,
    string Email,
    string Name,
    string? Bio,
    string Role);

public class SaveUserStateActivity(AuditDbContext db)
{
    [Activity("SaveUserState")]
    public async Task ExecuteAsync(SaveUserStateRequest request)
    {
        var existing = await db.Users.FindAsync(request.UserId);
        if (existing is null)
        {
            db.Users.Add(new User
            {
                Id = request.UserId,
                Email = request.Email,
                Name = request.Name,
                Bio = request.Bio,
                Role = request.Role,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.Email = request.Email;
            existing.Name = request.Name;
            existing.Bio = request.Bio;
            existing.Role = request.Role;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync();
    }
}
