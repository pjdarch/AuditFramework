using System.Net.Http.Json;
using AuditFramework.Web.Auth;

namespace AuditFramework.Web.Services;

public class IdentityApiClient(
    HttpClient http,
    TokenStore store,
    BrowserAuthSessionStore browserAuthSessionStore,
    IIdentityErrorParser errorParser
) : AuthenticatedApiClientBase(http, store, errorParser), IIdentityApiClient
{
    public async Task<(bool ok, string? error)> RegisterAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var response = await Http.PostAsJsonAsync("/register", new { email, password }, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await ErrorParser.ReadErrorAsync(response, ct));
    }

    public async Task<(bool ok, string? error)> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var response = await Http.PostAsJsonAsync("/login", new { email, password }, ct);
        if (!response.IsSuccessStatusCode)
            return (false, await ErrorParser.ReadErrorAsync(response, ct));

        var token = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        if (token is null)
            return (false, "Something went wrong. Please try again.");

        store.SetAuthenticatedSession(token.AccessToken, token.RefreshToken, token.ExpiresIn, email);
        await browserAuthSessionStore.PersistAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> ChangePasswordAsync(
        string oldPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/manage/info")
        {
            Content = JsonContent.Create(new { oldPassword, newPassword }),
        };
        return await SendAsync(request, ct);
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        store.Clear();
        await browserAuthSessionStore.ClearAsync(ct);
    }

    private record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType);
}

