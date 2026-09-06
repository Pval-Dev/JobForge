using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobForge.Persistence;

using JobForge.Job;
using JobForge.Users;
using JobForge.Audit;
using JobForge.Schedule;
using JobForge.Retry;
using JobForge.Webhooks;

public class JobForgeDbContext : DbContext
{
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<RetryAttempt> RetryAttempts { get; set; } = null!;
    public DbSet<Webhook> Webhooks { get; set; } = null!;

    public JobForgeDbContext(DbContextOptions<JobForgeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasKey(user => user.Id);
        modelBuilder.Entity<User>().Property(user => user.Username).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<User>().Property(user => user.NormalizedUsername).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<User>().HasIndex(user => user.NormalizedUsername).IsUnique();
        modelBuilder.Entity<User>().Property(user => user.PasswordHash).HasMaxLength(512);

        modelBuilder.Entity<Job>().HasKey(job => job.Id);
        modelBuilder.Entity<Job>().Property(job => job.JobName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Job>().Property(job => job.Payload).IsRequired();
        modelBuilder.Entity<Job>().Property(job => job.JobCode).HasConversion(jobCode => jobCode.Value, value => new JobCode(value)).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Job>().Property(job => job.Priority).HasConversion(priority => priority.Priority, value => new JobPriority(value));
        modelBuilder.Entity<Job>().Property(job => job.Result).HasConversion(
            result => result == null ? null : JsonSerializer.Serialize(result, (JsonSerializerOptions?)null),
            value => value == null ? null : JsonSerializer.Deserialize<JobResult>(value, (JsonSerializerOptions?)null)
        ).HasColumnType("jsonb");
        modelBuilder.Entity<Job>().Property(job => job.QueuedAt);

        modelBuilder.Entity<AuditEntry>().HasKey(entry => entry.Id);
        modelBuilder.Entity<AuditEntry>().Property(entry => entry.Message).IsRequired().HasMaxLength(500);
        modelBuilder.Entity<AuditEntry>().Property(entry => entry.EventType);
        modelBuilder.Entity<AuditEntry>().Property(entry => entry.EntityId);
        modelBuilder.Entity<AuditEntry>().Property(entry => entry.UserId);
        modelBuilder.Entity<AuditEntry>().Property(entry => entry.OccurredAt);

        modelBuilder.Entity<Schedule>().HasKey(schedule => schedule.Id);
        modelBuilder.Entity<Schedule>().Property(schedule => schedule.JobId);
        modelBuilder.Entity<Schedule>().Property(schedule => schedule.Type);
        modelBuilder.Entity<Schedule>().Property(schedule => schedule.RunAt);
        modelBuilder.Entity<Schedule>().Property(schedule => schedule.CreatedAt);
        modelBuilder.Entity<Schedule>().Property(schedule => schedule.IsActive);

        modelBuilder.Entity<RetryAttempt>().HasKey(attempt => new { attempt.JobId, attempt.AttemptNumber });
        modelBuilder.Entity<RetryAttempt>().Property(attempt => attempt.ErrorMessage).IsRequired().HasMaxLength(500);
        modelBuilder.Entity<RetryAttempt>().Property(attempt => attempt.FailedAt);
        modelBuilder.Entity<RetryAttempt>().Property(attempt => attempt.NextRetryAt);

        modelBuilder.Entity<Webhook>().HasKey(webhook => webhook.Id);
        modelBuilder.Entity<Webhook>().Property(webhook => webhook.Url).IsRequired().HasMaxLength(500);
        modelBuilder.Entity<Webhook>().Property(webhook => webhook.OwnerId);
        modelBuilder.Entity<Webhook>().Property(webhook => webhook.Event);
        modelBuilder.Entity<Webhook>().Property(webhook => webhook.IsActive);
        modelBuilder.Entity<Webhook>().Property(webhook => webhook.CreatedAt);
    }
}