using System.Security.Claims;

namespace AuditFramework.Web.Auth;

public class TokenStore
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public ClaimsPrincipal Principal { get; private set; } = new(new ClaimsIdentity());

    public event Action? Changed;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

    public void SetAuthenticatedSession(
        string accessToken,
        string refreshToken,
        int expiresInSeconds,
        string email
    )
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
        Principal = CreatePrincipal(email);
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        ExpiresAt = null;
        Principal = new ClaimsPrincipal(new ClaimsIdentity());
        Changed?.Invoke();
    }

    public StoredAuthSession? CreateSnapshot()
    {
        var email = Principal.FindFirstValue(ClaimTypes.Email);
        if (
            string.IsNullOrWhiteSpace(AccessToken)
            || string.IsNullOrWhiteSpace(RefreshToken)
            || ExpiresAt is null
            || string.IsNullOrWhiteSpace(email)
        )
        {
            return null;
        }

        return new StoredAuthSession(AccessToken, RefreshToken, ExpiresAt.Value, email);
    }

    public void Restore(StoredAuthSession session)
    {
        AccessToken = session.AccessToken;
        RefreshToken = session.RefreshToken;
        ExpiresAt = session.ExpiresAt;
        Principal = CreatePrincipal(session.Email);
        Changed?.Invoke();
    }

    private static ClaimsPrincipal CreatePrincipal(string email)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Email, email)],
            authenticationType: "Bearer"
        );

        return new ClaimsPrincipal(identity);
    }
}
