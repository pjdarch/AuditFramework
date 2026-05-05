namespace AuditFramework.ApiService.Temporal.Workflows;

public class UserProfileState
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string Role { get; set; } = "user";

    public UserProfileState Clone() => new()
    {
        UserId = UserId,
        Email  = Email,
        Name   = Name,
        Bio    = Bio,
        Role   = Role
    };

    public void Apply(UpdateUserProfileRequest request)
    {
        if (request.NewName  is not null) Name  = request.NewName;
        if (request.NewBio   is not null) Bio   = request.NewBio;
        if (request.NewEmail is not null) Email = request.NewEmail;
        if (request.NewRole  is not null) Role  = request.NewRole;
    }
}
