using System.Text.Json;

namespace AuditFramework.ApiService.Data.Models;

public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = "user";
    public Guid ResourceId { get; set; }
    public JsonDocument? OldResource { get; set; }
    public JsonDocument NewResource { get; set; } = null!;
    public JsonDocument? Metadata { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
