using System.Net.Http.Json;
using System.Text.Json;

namespace AuditFramework.Web;

public class TemporalProfileApiClient(HttpClient httpClient)
{
    public async Task<List<UserProfile>> GetUsersAsync(CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<List<UserProfile>>("/users", ct) ?? [];

    public async Task<List<AuditEventItem>> GetAuditEventsAsync(Guid? resourceId = null, CancellationToken ct = default)
    {
        var url = resourceId.HasValue ? $"/audit/events?resourceId={resourceId}" : "/audit/events";
        return await httpClient.GetFromJsonAsync<List<AuditEventItem>>(url, ct) ?? [];
    }

    public async Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<UserProfile>($"/users/{userId}/profile", ct);
    }

    public async Task<UserProfile?> UpdateProfileAsync(
        Guid userId, Guid actorId, string actorRole,
        string? name, string? bio, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/users/{userId}/profile")
        {
            Content = JsonContent.Create(new { name, bio })
        };
        request.Headers.Add("X-User-Id", actorId.ToString());
        request.Headers.Add("X-User-Role", actorRole);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfile>(ct);
    }
}

public record AuditEventItem(
    Guid Id,
    Guid ActorId,
    string Action,
    string ResourceType,
    Guid ResourceId,
    JsonElement? OldResource,
    JsonElement NewResource,
    JsonElement? Metadata,
    DateTimeOffset OccurredAt);

public record UserProfile(
    Guid Id,
    string Email,
    string Name,
    string? Bio,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
