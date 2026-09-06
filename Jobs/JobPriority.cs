namespace JobForge.Job;

public enum JobPriorityLevel
{
    Low,
    Normal,
    High,
    Critical
}

public class JobPriority
{
    public JobPriorityLevel Priority { get; }
    public DateTimeOffset AssignedAt { get; }

    public JobPriority(JobPriorityLevel priority)
    {
        if (!Enum.IsDefined(priority))
            throw new ArgumentOutOfRangeException(nameof(priority), "Invalid job priority.");

        Priority = priority;
        AssignedAt = DateTimeOffset.UtcNow;
    }
}