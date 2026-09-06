namespace JobForge.Schedule;

using JobForge.Job;
using JobForge.Queues;
using JobForge.Audit;
using JobForge.Persistence;

public class SchedulerService
{
    private readonly QueueService _queueService;
    private readonly IJobRepository _jobRepository;
    private readonly AuditService _auditService;
    private readonly ScheduleRepository _scheduleRepository;

    public SchedulerService(QueueService queueService, IJobRepository jobRepository, AuditService auditService, ScheduleRepository scheduleRepository)
    {
        _queueService = queueService;
        _jobRepository = jobRepository;
        _auditService = auditService;
        _scheduleRepository = scheduleRepository;
    }

    public void ScheduleJob(Job job, DateTimeOffset runAt, ScheduleType type)
    {
        if (type == ScheduleType.Delayed)
        {
            job.Schedule();
            _auditService.Record(AuditEventType.JobScheduled, job.Id, null, $"Job '{job.JobName}' was scheduled for {runAt}.");
        }

        _jobRepository.Update(job);
        _scheduleRepository.Add(new Schedule(job.Id, type, runAt));
    }

    public void ProcessDueJobs(DateTimeOffset now)
    {
        foreach (var schedule in _scheduleRepository.GetDue(now))
        {
            var job = _jobRepository.GetById(schedule.JobId)
                ?? throw new InvalidOperationException("Scheduled job does not exist.");
            _queueService.Enqueue(job);
            schedule.Deactivate();
            _scheduleRepository.Update(schedule);
        }
    }
}