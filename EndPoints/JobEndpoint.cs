namespace JobForge.EndPoints;

using System.Security.Claims;

using JobForge.Job;
using JobForge.Queues;
using JobForge.Users;


public record CreateJobRequest(
    string Code,
    string Name,
    string Payload,
    JobPriorityLevel Priority
);


public record JobResponse(
    Guid Id,
    string Code,
    string Name,
    string Payload,
    JobPriorityLevel Priority,
    Guid OwnerId,
    JobStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? QueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    JobResult? Result
);


public static class JobEndPoint
{
    public static void MapJobEndPoints(
        this WebApplication app)
    {
        var jobs =
            app.MapGroup("/jobs");

        jobs.MapPost("", CreateJob);
        jobs.MapGet("", GetJobs);
        jobs.MapGet("/{id:guid}", GetJob);
        jobs.MapPost("/{id:guid}/queue", QueueJob);
        jobs.MapDelete("/{id:guid}", CancelJob);
    }

    private static IResult CreateJob(
        CreateJobRequest request,
        ClaimsPrincipal principal,
        JobService jobService)
    {
        Guid ownerId = principal.GetUserId();

        Job job = jobService.CreateJob(
            new JobCode(request.Code),
            request.Name,
            request.Payload,
            request.Priority,
            ownerId
        );

        return Results.Created(
            $"/jobs/{job.Id}",
            ToResponse(job)
        );
    }

    private static IResult GetJobs(
        ClaimsPrincipal principal,
        JobService jobService)
    {
        Guid userId = principal.GetUserId();
        IEnumerable<Job> jobs = jobService.GetJobs();

        if (!principal.IsAdmin())
            jobs = jobs.Where(job => job.OwnerId == userId);

        return Results.Ok(jobs.Select(ToResponse));
    }

    private static IResult GetJob(
        Guid id,
        ClaimsPrincipal principal,
        JobService jobService,
        UserService userService)
    {
        Guid userId = principal.GetUserId();
        Job job = jobService.GetJob(id);

        if (!userService.CanAccessJob(userId, job))
            return Results.Forbid();

        return Results.Ok(ToResponse(job));
    }

    private static IResult QueueJob(
        Guid id,
        ClaimsPrincipal principal,
        JobService jobService,
        UserService userService,
        QueueService queueService)
    {
        Guid userId = principal.GetUserId();
        Job job = jobService.GetJob(id);

        if (!userService.CanAccessJob(userId, job))
            return Results.Forbid();

        queueService.Enqueue(job);

        return Results.Accepted(
            $"/jobs/{job.Id}",
            ToResponse(job)
        );
    }

    private static IResult CancelJob(
        Guid id,
        ClaimsPrincipal principal,
        JobService jobService,
        UserService userService)
    {
        Guid userId = principal.GetUserId();
        Job job = jobService.GetJob(id);

        if (!userService.CanCancelJob(userId, job))
            return Results.Forbid();

        Job cancelled = jobService.CancelJob(id);
        return Results.Ok(ToResponse(cancelled));
    }

    private static JobResponse ToResponse(Job job)
    {
        return new JobResponse(
            job.Id,
            job.JobCode.Value,
            job.JobName,
            job.Payload,
            job.Priority.Priority,
            job.OwnerId,
            job.Status,
            job.CreatedAt,
            job.QueuedAt,
            job.StartedAt,
            job.CompletedAt,
            job.Result
        );
    }
}