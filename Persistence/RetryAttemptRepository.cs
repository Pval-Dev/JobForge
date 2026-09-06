namespace JobForge.Persistence;

using JobForge.Retry;

public class RetryAttemptRepository
{
    private readonly JobForgeDbContext _context;
    public RetryAttemptRepository(JobForgeDbContext context) => _context = context;
    public void Add(RetryAttempt attempt) { _context.RetryAttempts.Add(attempt); _context.SaveChanges(); }
    public IEnumerable<RetryAttempt> GetByJob(Guid jobId) => _context.RetryAttempts.Where(attempt => attempt.JobId == jobId).OrderBy(attempt => attempt.AttemptNumber).ToList();
    public int CountByJob(Guid jobId) => _context.RetryAttempts.Count(attempt => attempt.JobId == jobId);
}