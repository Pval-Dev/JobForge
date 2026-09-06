namespace JobForge.Job;

public enum JobStatus
{
    Created,
    Scheduled,
    Queued,
    Running,
    Retrying,
    Succeeded,
    Failed,
    Cancelled
}