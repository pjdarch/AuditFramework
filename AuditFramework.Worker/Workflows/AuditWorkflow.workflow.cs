using AuditFramework.Worker.Activities;
using Temporalio.Workflows;

namespace AuditFramework.Worker.Workflows;

[Workflow]
public class AuditWorkflow
{
    [WorkflowRun]
    public async Task<string> RunAsync(AuditWorkflowInput input)
    {
        throw new NotImplementedException();
    }
}

public record AuditWorkflowInput(string UserId, string Action, string Resource);
