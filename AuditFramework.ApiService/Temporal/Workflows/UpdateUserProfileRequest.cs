namespace AuditFramework.ApiService.Temporal.Workflows;

public record UpdateUserProfileRequest(
    Guid ActorId,
    string ActorRole,
    string? NewName,
    string? NewBio,
    string? NewEmail,
    string? NewRole
);
