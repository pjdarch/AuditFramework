namespace AuditFramework.Web.Services;

public interface IIdentityApiClient
{
    Task<(bool ok, string? error)> RegisterAsync(string email, string password, CancellationToken ct = default);
    Task<(bool ok, string? error)> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<(bool ok, string? error)> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
