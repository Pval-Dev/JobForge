namespace JobForge.Persistence;

using JobForge.Job;

public class JobRepository : IJobRepository
{
    private readonly JobForgeDbContext _context;
    public JobRepository(JobForgeDbContext context) => _context = context;
    public void Add(Job job) { _context.Jobs.Add(job); _context.SaveChanges(); }
    public void Update(Job job) { _context.Jobs.Update(job); _context.SaveChanges(); }
    public Job? GetById(Guid id) => _context.Jobs.Find(id);
    public IEnumerable<Job> GetAll() => _context.Jobs.ToList();
    public IEnumerable<Job> GetByStatus(JobStatus status) => _context.Jobs.Where(job => job.Status == status).ToList();
}