namespace JobForge.Audit;

public enum AuditEventType
{
    JobCreated,
    JobScheduled,
    JobQueued,
    JobStarted,
    JobSucceeded,
    JobFailed,
    JobRetrying,
    JobCancelled,

    WorkerCreated,
    WorkerAssigned,
    WorkerReleased,

    UserCreated,
    UserDeactivated,
    UserActivated,
    UserRoleChanged
}