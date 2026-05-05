using System.Net.Http.Json;
using AuditFramework.Web.Auth;

namespace AuditFramework.Web.Services;

public class ProfileApiClient(HttpClient http, TokenStore store, IIdentityErrorParser errorParser)
    : AuthenticatedApiClientBase(http, store, errorParser), IProfileApiClient
{
    public async Task<ProfileDto?> GetAsync(CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        AttachAuth(request);
        var response = await Http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDto>(ct);
    }

    public async Task<(bool ok, string? error, ProfileDto? profile)> UpdateAsync(
        UpdateProfileRequest profile,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile")
        {
            Content = JsonContent.Create(profile),
        };
        return await SendAsync<ProfileDto>(request, ct);
    }
}

public record ProfileDto(
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country
);

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country
);
