namespace JobForge.Schedule;

public class Schedule
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public ScheduleType Type { get; private set; }
    public DateTimeOffset RunAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Schedule() { }

    public Schedule(Guid jobId, ScheduleType type, DateTimeOffset runAt)
    {
        Id = Guid.NewGuid();
        JobId = jobId;
        Type = type;
        RunAt = runAt;
        CreatedAt = DateTimeOffset.UtcNow;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) throw new InvalidOperationException("Schedule is already inactive.");
        IsActive = false;
    }
}