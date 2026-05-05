using Microsoft.AspNetCore.Components.Authorization;

namespace AuditFramework.Web.Auth;

public class CircuitAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly TokenStore _store;

    public CircuitAuthStateProvider(TokenStore store)
    {
        _store = store;
        _store.Changed += OnStoreChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_store.Principal));

    private void OnStoreChanged() =>
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_store.Principal))
        );

    public void Dispose() => _store.Changed -= OnStoreChanged;
}
