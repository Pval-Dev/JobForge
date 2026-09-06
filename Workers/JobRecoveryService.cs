namespace JobForge.Workers;

using JobForge.Job;
using JobForge.Retry;

public sealed class JobRecoveryService
{
    private readonly IJobRepository _repository;
    private readonly RetryService _retryService;
    private readonly ILogger<JobRecoveryService> _logger;

    public JobRecoveryService(IJobRepository repository, RetryService retryService, ILogger<JobRecoveryService> logger)
    {
        _repository = repository;
        _retryService = retryService;
        _logger = logger;
    }

    public int RecoverInterruptedJobs()
    {
        var interruptedJobs = _repository.GetByStatus(JobStatus.Running).ToList();
        foreach (Job job in interruptedJobs)
        {
            var result = new JobResult(false, "Job execution was interrupted by a server restart.");
            _retryService.HandleFailure(job, result);
            _logger.LogWarning("Recovered interrupted job {JobId}.", job.Id);
        }
        return interruptedJobs.Count;
    }
}