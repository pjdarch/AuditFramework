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

    public void SetTokens(string accessToken, string refreshToken, int expiresInSeconds)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
    }

    public void SetPrincipal(string email)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Email, email)],
            authenticationType: "Bearer"
        );
        Principal = new ClaimsPrincipal(identity);
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
}
