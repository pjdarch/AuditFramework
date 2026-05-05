namespace AuditFramework.ApiService.Temporal.Workflows;

public record CreateUserRequest(
    Guid ActorId,
    string ActorRole,
    Guid NewUserId,
    string Email,
    string Name,
    string? Bio,
    string Role
);
