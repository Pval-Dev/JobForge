namespace JobForge.Workers;

using JobForge.Job;

public class JobExecutor
{
    private readonly IEnumerable<JobHandler> _handlers;
    public JobExecutor(IEnumerable<JobHandler> handlers) => _handlers = handlers;

    public JobResult Execute(Job job)
    {
        var handler = _handlers.FirstOrDefault(h => h.JobType == job.JobCode.Value);
        if (handler is null) return new JobResult(false, $"No handler found for job type: {job.JobCode.Value}");
        return handler.Handle(job);
    }
}