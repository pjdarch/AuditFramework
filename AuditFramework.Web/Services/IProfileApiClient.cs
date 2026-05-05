namespace AuditFramework.Web.Services;

public interface IProfileApiClient
{
    Task<ProfileDto?> GetAsync(CancellationToken ct = default);
    Task<(bool ok, string? error, ProfileDto? profile)> UpdateAsync(UpdateProfileRequest profile, CancellationToken ct = default);
}
