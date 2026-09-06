namespace JobForge.Workers;

using JobForge.Job;

public class JobHandler
{
    public string JobType { get; }
    private readonly Func<Job, JobResult> _handle;

    public JobHandler(string jobType, Func<Job, JobResult> handle)
    {
        JobType = jobType;
        _handle = handle;
    }

    public JobResult Handle(Job job) => _handle(job);
}