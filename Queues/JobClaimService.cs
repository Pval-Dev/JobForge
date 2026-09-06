namespace JobForge.Queues;

using JobForge.Job;
using JobForge.Persistence;
using Microsoft.EntityFrameworkCore;

public class JobClaimService
{
    private readonly JobForgeDbContext _context;
    private readonly JobService _jobService;

    public JobClaimService(JobForgeDbContext context, JobService jobService)
    {
        _context = context;
        _jobService = jobService;
    }

    public async Task<Job?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var jobs = await _context.Jobs
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Jobs"
                WHERE "Status" = {(int)JobStatus.Queued}
                ORDER BY "Priority" DESC, "QueuedAt" ASC
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        Job? job = jobs.FirstOrDefault();
        if (job is null) return null;

        _jobService.MarkJobAsRunning(job.Id);
        await transaction.CommitAsync(cancellationToken);
        return job;
    }
}