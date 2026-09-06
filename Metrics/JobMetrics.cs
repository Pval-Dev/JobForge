namespace JobForge.Metrics;

public class JobMetrics
{
    public int TotalJobs { get; }
    public int CreatedJobs { get; }
    public int ScheduledJobs { get; }
    public int QueuedJobs { get; }
    public int RunningJobs { get; }
    public int RetryingJobs { get; }
    public int SucceededJobs { get; }
    public int FailedJobs { get; }
    public int CancelledJobs { get; }
    public int ActiveJobs { get; }
    public int CompletedJobs { get; }
    public double SuccessRate { get; }
    public double FailureRate { get; }

    public JobMetrics(int totalJobs, int createdJobs, int scheduledJobs, int queuedJobs, int runningJobs, int retryingJobs, int succeededJobs, int failedJobs, int cancelledJobs)
    {
        TotalJobs = totalJobs;
        CreatedJobs = createdJobs;
        ScheduledJobs = scheduledJobs;
        QueuedJobs = queuedJobs;
        RunningJobs = runningJobs;
        RetryingJobs = retryingJobs;
        SucceededJobs = succeededJobs;
        FailedJobs = failedJobs;
        CancelledJobs = cancelledJobs;
        ActiveJobs = createdJobs + scheduledJobs + queuedJobs + runningJobs + retryingJobs;
        CompletedJobs = succeededJobs + failedJobs + cancelledJobs;
        SuccessRate = totalJobs == 0 ? 0 : (double)succeededJobs / totalJobs * 100;
        FailureRate = totalJobs == 0 ? 0 : (double)failedJobs / totalJobs * 100;
    }
}