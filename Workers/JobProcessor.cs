namespace JobForge.Workers;

using JobForge.Audit;
using JobForge.Job;
using JobForge.Queues;
using JobForge.Retry;

public class JobProcessor
{
    private readonly QueueService _queueService;
    private readonly AuditService _auditService;
    private readonly JobService _jobService;
    private readonly RetryService _retryService;
    private readonly JobExecutor _executor;

    public JobProcessor(QueueService queueService, AuditService auditService, JobService jobService, RetryService retryService, JobExecutor jobExecutor)
    {
        _queueService = queueService;
        _auditService = auditService;
        _jobService = jobService;
        _retryService = retryService;
        _executor = jobExecutor;
    }

    public async Task ProcessNextJobAsync(Worker worker, CancellationToken cancellationToken)
    {
        Job? nextJob = await _queueService.DequeueNextAsync(cancellationToken);
        if (nextJob is null) return;

        worker.AssignJob(nextJob);
        _auditService.Record(AuditEventType.WorkerAssigned, worker.Id, null, $"Worker '{worker.Name}' was assigned to job '{nextJob.JobName}'.");

        JobResult result = _executor.Execute(nextJob);
        if (result.Success) _jobService.CompleteJob(nextJob.Id, result);
        else _retryService.HandleFailure(nextJob, result);

        worker.CompleteJob(nextJob);
        _auditService.Record(AuditEventType.WorkerReleased, worker.Id, null, $"Worker '{worker.Name}' was released from job '{nextJob.JobName}'.");
    }
}