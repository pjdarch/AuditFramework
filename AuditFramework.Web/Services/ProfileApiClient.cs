using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuditFramework.Web.Auth;

namespace AuditFramework.Web.Services;

public class ProfileApiClient(HttpClient http, TokenStore store)
{
    public async Task<ProfileDto?> GetAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        AttachAuth(req);
        var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProfileDto>(ct);
    }

    public async Task<(bool ok, string? error, ProfileDto? profile)> UpdateAsync(
        string? firstName,
        string? lastName,
        CancellationToken ct = default
    )
    {
        using var req = new HttpRequestMessage(HttpMethod.Put, "/api/profile")
        {
            Content = JsonContent.Create(new { firstName, lastName }),
        };
        AttachAuth(req);
        var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return (false, string.IsNullOrWhiteSpace(body) ? resp.ReasonPhrase : body, null);
        }
        var dto = await resp.Content.ReadFromJsonAsync<ProfileDto>(ct);
        return (true, null, dto);
    }

    private void AttachAuth(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(store.AccessToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", store.AccessToken);
        }
    }
}

public record ProfileDto(string? Email, string? FirstName, string? LastName);
