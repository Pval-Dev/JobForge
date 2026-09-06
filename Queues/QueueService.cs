namespace JobForge.Queues;

using JobForge.Audit;
using JobForge.Job;

public class QueueService
{
    private readonly IJobRepository _repository;
    private readonly AuditService _auditService;
    private readonly JobClaimService _claimService;

    public QueueService(IJobRepository repository, AuditService auditService, JobClaimService claimService)
    {
        _repository = repository;
        _auditService = auditService;
        _claimService = claimService;
    }

    public void Enqueue(Job job)
    {
        job.Enqueue();
        _repository.Update(job);
        _auditService.Record(AuditEventType.JobQueued, job.Id, null, $"Job '{job.JobName}' was added to the queue.");
    }

    public Task<Job?> DequeueNextAsync(CancellationToken cancellationToken)
        => _claimService.ClaimNextAsync(cancellationToken);
}