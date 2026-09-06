namespace JobForge.Retry;

using JobForge.Schedule;
using JobForge.Job;
using JobForge.Audit;
using JobForge.Persistence;

public class RetryService
{
    private readonly JobService _jobService;
    private readonly RetryPolicy _policy;
    private readonly SchedulerService _scheduler;
    private readonly AuditService _auditService;
    private readonly RetryAttemptRepository _retryRepository;

    public RetryService(JobService jobService, RetryPolicy policy, SchedulerService scheduler, AuditService auditService, RetryAttemptRepository retryRepository)
    {
        _jobService = jobService;
        _policy = policy;
        _scheduler = scheduler;
        _auditService = auditService;
        _retryRepository = retryRepository;
    }

    public int GetAttempts(Job job) => _retryRepository.CountByJob(job.Id);
    public void AddAttempt(RetryAttempt attempt) => _retryRepository.Add(attempt);

    public void HandleFailure(Job job, JobResult result)
    {
        int currentAttempts = GetAttempts(job);
        if (!_policy.CanRetry(currentAttempts))
        {
            _jobService.FailJob(job.Id, result);
            return;
        }

        int nextAttemptNumber = currentAttempts + 1;
        int delay = _policy.CalculateDelay(nextAttemptNumber);
        DateTimeOffset nextRetryAt = DateTimeOffset.UtcNow.AddSeconds(delay);
        job.Retry();

        AddAttempt(new RetryAttempt(job.Id, nextAttemptNumber, result.Message, nextRetryAt));
        _scheduler.ScheduleJob(job, nextRetryAt, ScheduleType.Retry);
        _auditService.Record(AuditEventType.JobRetrying, job.Id, null, $"Job '{job.JobName}' will retry with attempt {nextAttemptNumber}.");
    }
}