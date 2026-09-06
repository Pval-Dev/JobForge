namespace JobForge.Job;

using JobForge.Users;
using JobForge.Audit;

public class JobService
{
    private readonly IJobRepository _repository;
    private readonly UserService _userService;
    private readonly AuditService _auditService;

    public JobService(IJobRepository repository, UserService userService, AuditService auditService)
    {
        _repository = repository;
        _userService = userService;
        _auditService = auditService;
    }

    public Job CreateJob(JobCode code, string name, string payload, JobPriorityLevel priority, Guid ownerId)
    {
        var user = _userService.GetUser(ownerId);
        if (user is null) throw new KeyNotFoundException("User does not exist.");
        if (!user.IsActive) throw new InvalidOperationException("User is inactive.");

        var job = new Job(code, name, payload, priority, ownerId);
        _repository.Add(job);
        _auditService.Record(AuditEventType.JobCreated, job.Id, ownerId, $"Job '{job.JobName}' was created.");
        return job;
    }

    public Job CancelJob(Guid id)
    {
        var job = _repository.GetById(id) ?? throw new KeyNotFoundException("Job does not exist.");
        job.Cancel();
        _repository.Update(job);
        _auditService.Record(AuditEventType.JobCancelled, job.Id, null, $"Job '{job.JobName}' was cancelled.");
        return job;
    }

    public Job GetJob(Guid id)
        => _repository.GetById(id) ?? throw new KeyNotFoundException("Job does not exist.");

    public IEnumerable<Job> GetJobs() => _repository.GetAll();

    public Job MarkJobAsRunning(Guid id)
    {
        var job = GetJob(id);
        job.Start();
        _repository.Update(job);
        _auditService.Record(AuditEventType.JobStarted, job.Id, null, $"Job '{job.JobName}' started.");
        return job;
    }

    public Job CompleteJob(Guid id, JobResult result)
    {
        var job = GetJob(id);
        job.Succeed(result);
        _repository.Update(job);
        _auditService.Record(AuditEventType.JobSucceeded, job.Id, null, $"Job '{job.JobName}' succeeded.");
        return job;
    }

    public Job FailJob(Guid id, JobResult result)
    {
        var job = GetJob(id);
        job.Fail(result);
        _repository.Update(job);
        _auditService.Record(AuditEventType.JobFailed, job.Id, null, $"Job '{job.JobName}' failed: {result.Message}");
        return job;
    }
}