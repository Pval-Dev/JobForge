namespace JobForge.Job;

using JobForge.Common;

public record JobCode
{
    public string Value { get; }

    public JobCode(string code)
    {
        StringValidator.Validate(code);
        Value = code;
    }
}

public class Job
{
    public JobCode JobCode { get; private set; }
    public JobPriority Priority { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid Id { get; private set; }
    public JobStatus Status { get; private set; }
    public string JobName { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? QueuedAt { get; private set; }
    public JobResult? Result { get; private set; }

    private Job()
    {
        JobCode = null!;
        JobName = null!;
        Payload = null!;
        Priority = null!;
    }

    public Job(JobCode code, string name, string payload, JobPriorityLevel priority, Guid ownerId)
    {
        StringValidator.Validate(name);
        if (name.Length > 200)
            throw new ArgumentException("Job name cannot exceed 200 characters.");

        Id = Guid.NewGuid();
        JobCode = code;
        JobName = name;
        Payload = payload;
        Priority = new JobPriority(priority);
        OwnerId = ownerId;
        Status = JobStatus.Created;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Schedule()
    {
        if (Status != JobStatus.Created)
            throw new InvalidOperationException("Job must be in Created state before scheduling.");
        Status = JobStatus.Scheduled;
    }

    public void Enqueue()
    {
        if (Status != JobStatus.Created && Status != JobStatus.Scheduled && Status != JobStatus.Retrying)
            throw new InvalidOperationException("Job must be Created, Scheduled, or Retrying before it can be queued.");
        Status = JobStatus.Queued;
        QueuedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        if (Status != JobStatus.Queued)
            throw new InvalidOperationException("Job must be Queued before it can start.");
        Status = JobStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Retry()
    {
        if (Status != JobStatus.Running)
            throw new InvalidOperationException("Job must be Running before retrying.");
        Status = JobStatus.Retrying;
    }

    public void Succeed(JobResult result)
    {
        if (Status != JobStatus.Running)
            throw new InvalidOperationException("Job must be Running before succeeding.");
        if (!result.Success)
            throw new ArgumentException("Cannot mark a job as Succeeded with a failed result.");
        Status = JobStatus.Succeeded;
        CompletedAt = DateTimeOffset.UtcNow;
        Result = result;
    }

    public void Fail(JobResult result)
    {
        if (Status != JobStatus.Running)
            throw new InvalidOperationException("Job must be Running before failing.");
        if (result.Success)
            throw new ArgumentException("Cannot mark a job as Failed with a successful result.");
        Status = JobStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        Result = result;
    }

    public void Cancel()
    {
        if (Status is JobStatus.Running or JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled)
            throw new InvalidOperationException("A running or terminal job cannot be cancelled.");
        CompletedAt = DateTimeOffset.UtcNow;
        Status = JobStatus.Cancelled;
    }
}