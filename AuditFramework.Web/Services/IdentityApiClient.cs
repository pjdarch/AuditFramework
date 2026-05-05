using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuditFramework.Web.Auth;

namespace AuditFramework.Web.Services;

public class IdentityApiClient(HttpClient http, TokenStore store)
{
    public async Task<(bool ok, string? error)> RegisterAsync(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        var resp = await http.PostAsJsonAsync("/register", new { email, password }, ct);
        return resp.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(resp, ct));
    }

    public async Task<(bool ok, string? error)> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        var resp = await http.PostAsJsonAsync("/login", new { email, password }, ct);
        if (!resp.IsSuccessStatusCode)
            return (false, await ReadErrorAsync(resp, ct));
        var token = await resp.Content.ReadFromJsonAsync<LoginResponse>(ct);
        if (token is null)
            return (false, "Empty token response");
        store.SetTokens(token.AccessToken, token.RefreshToken, token.ExpiresIn);
        store.SetPrincipal(email);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> ChangePasswordAsync(
        string oldPassword,
        string newPassword,
        CancellationToken ct = default
    )
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/manage/info")
        {
            Content = JsonContent.Create(new { oldPassword, newPassword }),
        };
        AttachAuth(req);
        var resp = await http.SendAsync(req, ct);
        return resp.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(resp, ct));
    }

    public void Logout() => store.Clear();

    private void AttachAuth(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(store.AccessToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", store.AccessToken);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(body) ? resp.ReasonPhrase ?? "Request failed" : body;
        }
        catch
        {
            return resp.ReasonPhrase ?? "Request failed";
        }
    }

    private record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType
    );
}
