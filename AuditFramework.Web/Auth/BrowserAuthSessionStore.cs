using Microsoft.JSInterop;

namespace AuditFramework.Web.Auth;

public class BrowserAuthSessionStore(IJSRuntime js, TokenStore tokenStore)
{
    private bool _isRestored;

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (_isRestored)
        {
            return;
        }

        _isRestored = true;

        var session = await js.InvokeAsync<StoredAuthSession?>(
            "auditAuth.get",
            cancellationToken
        );

        if (session is null)
        {
            return;
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await ClearAsync(cancellationToken);
            return;
        }

        tokenStore.Restore(session);
    }

    public async Task PersistAsync(CancellationToken cancellationToken = default)
    {
        var session = tokenStore.CreateSnapshot();
        if (session is null)
        {
            return;
        }

        await js.InvokeVoidAsync("auditAuth.set", cancellationToken, session);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await js.InvokeVoidAsync("auditAuth.clear", cancellationToken);
    }
}
