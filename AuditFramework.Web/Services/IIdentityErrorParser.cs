namespace AuditFramework.Web.Services;

public interface IIdentityErrorParser
{
    Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct = default);
}
