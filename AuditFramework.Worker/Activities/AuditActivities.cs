using Temporalio.Activities;

namespace AuditFramework.Worker.Activities;

public class AuditActivities
{
    [Activity]
    public string RecordAuditEntry(string userId, string action, string resource)
    {
        throw new NotImplementedException();
    }

    [Activity]
    public string NotifyAuditComplete(string entryId)
    {
        throw new NotImplementedException();
    }
}
