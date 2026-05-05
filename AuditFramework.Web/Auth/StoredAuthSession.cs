namespace AuditFramework.Web.Auth;

public sealed record StoredAuthSession(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    string Email
);
