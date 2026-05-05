using System.Net.Http.Headers;
using AuditFramework.Web.Auth;

namespace AuditFramework.Web.Services;

public abstract class AuthenticatedApiClientBase(HttpClient http, TokenStore store, IIdentityErrorParser errorParser)
{
    protected HttpClient Http { get; } = http;
    protected IIdentityErrorParser ErrorParser { get; } = errorParser;

    protected void AttachAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(store.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", store.AccessToken);
    }

    protected async Task<(bool ok, string? error)> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        AttachAuth(request);
        var response = await Http.SendAsync(request, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await ErrorParser.ReadErrorAsync(response, ct));
    }

    protected async Task<(bool ok, string? error, T? data)> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        AttachAuth(request);
        var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return (false, await ErrorParser.ReadErrorAsync(response, ct), default);
        var data = await response.Content.ReadFromJsonAsync<T>(ct);
        return (true, null, data);
    }
}
