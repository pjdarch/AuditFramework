using AuditFramework.ApiService.Temporal.Activities;
using Temporalio.Workflows;

namespace AuditFramework.ApiService.Temporal.Workflows;

[Workflow]
public class UserWorkflow : IUserWorkflow
{
    private const int ContinueAsNewThreshold = 100;

    private static readonly ActivityOptions DefaultActivityOptions = new()
    {
        StartToCloseTimeout = TimeSpan.FromSeconds(30)
    };

    private UserProfileState _state = new();
    private int _updateCount;

    [WorkflowRun]
    public async Task RunAsync(Guid userId, UserProfileState? initialState = null)
    {
        if (initialState is not null)
        {
            _state = initialState;
        }
        else
        {
            _state.UserId = userId;
        }

        await Workflow.WaitConditionAsync(
            () => _updateCount >= ContinueAsNewThreshold,
            Timeout.InfiniteTimeSpan);

        throw Workflow.CreateContinueAsNewException<IUserWorkflow>(
            wf => wf.RunAsync(_state.UserId, _state.Clone()));
    }

    [WorkflowUpdate]
    public async Task<UserProfileState> UpdateProfileAsync(UpdateUserProfileRequest request)
    {
        var oldState = _state.Clone();

        var newState = _state.Clone();
        newState.Apply(request);

        await Workflow.ExecuteActivityAsync(
            (SaveUserStateActivity a) => a.ExecuteAsync(new SaveUserStateRequest(
                newState.UserId,
                newState.Email,
                newState.Name,
                newState.Bio,
                newState.Role)),
            DefaultActivityOptions);

        await Workflow.ExecuteActivityAsync(
            (WriteAuditEventActivity a) => a.ExecuteAsync(new WriteAuditEventRequest(
                request.ActorId,
                "UpdateProfile",
                "User",
                newState.UserId,
                oldState,
                newState,
                new { request.ActorRole })),
            DefaultActivityOptions);

        _state = newState;
        _updateCount++;

        return _state.Clone();
    }
}
