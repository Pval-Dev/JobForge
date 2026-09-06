namespace JobForge.Workers;

using JobForge.Common;
using JobForge.Job;

public class Worker
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public WorkerStatus Status { get; private set; }
    public Job? CurrentJob { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? LastActivity { get; private set; }

    public Worker(string name)
    {
        StringValidator.Validate(name);
        Name = name;
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Status = WorkerStatus.Idle;
    }

    public void SetOffline()
    {
        if (Status != WorkerStatus.Idle) throw new InvalidOperationException("Only an idle worker can be set offline.");
        Status = WorkerStatus.Offline;
        LastActivity = DateTimeOffset.UtcNow;
    }

    public void SetOnline()
    {
        if (Status != WorkerStatus.Offline) throw new InvalidOperationException("Only an offline worker can be set online.");
        Status = WorkerStatus.Idle;
        LastActivity = DateTimeOffset.UtcNow;
    }

    public void AssignJob(Job job)
    {
        if (Status != WorkerStatus.Idle) throw new InvalidOperationException("Only an idle worker can accept a job.");
        CurrentJob = job;
        Status = WorkerStatus.Busy;
        LastActivity = DateTimeOffset.UtcNow;
    }

    public void CompleteJob(Job job)
    {
        if (Status != WorkerStatus.Busy) throw new InvalidOperationException("Worker must be busy before completing a job.");
        if (CurrentJob is null) throw new InvalidOperationException("Worker does not have a current job.");
        if (job.Id != CurrentJob.Id) throw new InvalidOperationException("The completed job must match the worker's current job.");
        CurrentJob = null;
        Status = WorkerStatus.Idle;
        LastActivity = DateTimeOffset.UtcNow;
    }
}