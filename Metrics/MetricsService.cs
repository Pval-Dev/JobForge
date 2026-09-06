namespace JobForge.Metrics;

using JobForge.Job;
using JobForge.Workers;

public class MetricsService
{
    private readonly JobService _jobs;
    private readonly WorkerRegistry _workers;

    public MetricsService(JobService jobService, WorkerRegistry workerRegistry)
    {
        _jobs = jobService;
        _workers = workerRegistry;
    }

    public JobMetrics GetJobMetrics()
    {
        var jobs = _jobs.GetJobs();
        int total = jobs.Count();
        int created = jobs.Count(job => job.Status == JobStatus.Created);
        int scheduled = jobs.Count(job => job.Status == JobStatus.Scheduled);
        int queued = jobs.Count(job => job.Status == JobStatus.Queued);
        int running = jobs.Count(job => job.Status == JobStatus.Running);
        int retrying = jobs.Count(job => job.Status == JobStatus.Retrying);
        int succeeded = jobs.Count(job => job.Status == JobStatus.Succeeded);
        int failed = jobs.Count(job => job.Status == JobStatus.Failed);
        int cancelled = jobs.Count(job => job.Status == JobStatus.Cancelled);
        return new JobMetrics(total, created, scheduled, queued, running, retrying, succeeded, failed, cancelled);
    }

    public WorkerMetrics GetWorkerMetrics()
    {
        var workers = _workers.GetAllWorkers();
        int total = workers.Count();
        int idle = workers.Count(worker => worker.Status == WorkerStatus.Idle);
        int busy = workers.Count(worker => worker.Status == WorkerStatus.Busy);
        int offline = workers.Count(worker => worker.Status == WorkerStatus.Offline);
        return new WorkerMetrics(total, idle, busy, offline);
    }
}