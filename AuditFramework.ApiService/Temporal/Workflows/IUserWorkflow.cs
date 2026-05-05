using Temporalio.Workflows;

namespace AuditFramework.ApiService.Temporal.Workflows;

[Workflow]
public interface IUserWorkflow
{
    [WorkflowRun]
    Task RunAsync(Guid userId, UserProfileState? initialState = null);

    [WorkflowUpdate]
    Task<UserProfileState> CreateUserAsync(CreateUserRequest request);

    [WorkflowUpdate]
    Task<UserProfileState> UpdateProfileAsync(UpdateUserProfileRequest request);
}
