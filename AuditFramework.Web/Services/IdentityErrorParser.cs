using System.Text.Json;

namespace AuditFramework.Web.Services;

public class IdentityErrorParser : IIdentityErrorParser
{
    public async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return "Incorrect email or password.";

        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return "Something went wrong. Please try again.";

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // ASP.NET Identity ProblemDetails: { "errors": { "Code": ["message"] } }
            if (root.TryGetProperty("errors", out var errors))
            {
                var messages = new List<string>();
                foreach (var prop in errors.EnumerateObject())
                    foreach (var msg in prop.Value.EnumerateArray())
                        messages.Add(Humanize(prop.Name, msg.GetString() ?? prop.Name));

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                return title.GetString()!;

            return "Something went wrong. Please try again.";
        }
        catch
        {
            return "Something went wrong. Please try again.";
        }
    }

    private static string Humanize(string code, string fallback) => code switch
    {
        "DuplicateUserName" or "DuplicateEmail"     => "An account with that email already exists.",
        "InvalidEmail"                               => "Please enter a valid email address.",
        "PasswordTooShort"                           => "Password must be at least 6 characters.",
        "PasswordRequiresNonAlphanumeric"            => "Password must contain at least one special character (e.g. ! @ # $).",
        "PasswordRequiresDigit"                      => "Password must contain at least one number.",
        "PasswordRequiresLower"                      => "Password must contain at least one lowercase letter.",
        "PasswordRequiresUpper"                      => "Password must contain at least one uppercase letter.",
        "PasswordRequiresUniqueChars"                => "Password must contain more unique characters.",
        "PasswordMismatch"                           => "Your current password is incorrect.",
        _                                            => fallback
    };
}
