namespace JobForge.Job;

public interface IJobRepository
{
    void Add(Job job);
    void Update(Job job);
    Job? GetById(Guid id);
    IEnumerable<Job> GetAll();
    IEnumerable<Job> GetByStatus(JobStatus status);
}