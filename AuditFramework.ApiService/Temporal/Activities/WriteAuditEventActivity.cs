using System.Text.Json;
using AuditFramework.ApiService.Data;
using AuditFramework.ApiService.Data.Models;
using Temporalio.Activities;

namespace AuditFramework.ApiService.Temporal.Activities;

public record WriteAuditEventRequest(
    Guid ActorId,
    string Action,
    string ResourceType,
    Guid ResourceId,
    object? OldResource,
    object NewResource,
    object? Metadata);

public class WriteAuditEventActivity(AuditDbContext db)
{
    [Activity]
    public async Task ExecuteAsync(WriteAuditEventRequest request)
    {
        var options = new JsonSerializerOptions { WriteIndented = false };

        db.Events.Add(new AuditEvent
        {
            Id = Guid.CreateVersion7(),
            ActorId = request.ActorId,
            Action = request.Action,
            ResourceType = request.ResourceType,
            ResourceId = request.ResourceId,
            OldResource = request.OldResource is null
                ? null
                : JsonDocument.Parse(JsonSerializer.Serialize(request.OldResource, options)),
            NewResource = JsonDocument.Parse(JsonSerializer.Serialize(request.NewResource, options)),
            Metadata = request.Metadata is null
                ? null
                : JsonDocument.Parse(JsonSerializer.Serialize(request.Metadata, options)),
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }
}
