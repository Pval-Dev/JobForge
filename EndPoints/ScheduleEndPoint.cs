namespace JobForge.EndPoints;

using System.Security.Claims;

using JobForge.Job;
using JobForge.Schedule;
using JobForge.Users;

public record ScheduleJobRequest(DateTimeOffset RunAt);

public record ScheduleJobResponse(
    Guid JobId,
    DateTimeOffset RunAt,
    ScheduleType Type
);

public static class ScheduleEndPoint
{
    public static void MapScheduleEndPoints(this WebApplication app)
    {
        app.MapPost(
            "/jobs/{id:guid}/schedule",
            ScheduleJob
        );
    }

    private static IResult ScheduleJob(
        Guid id,
        ScheduleJobRequest request,
        ClaimsPrincipal principal,
        JobService jobService,
        UserService userService,
        SchedulerService schedulerService)
    {
        Guid userId = principal.GetUserId();
        Job job = jobService.GetJob(id);

        if (!userService.CanAccessJob(userId, job))
            return Results.Forbid();

        if (request.RunAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException(
                "Delayed jobs must be scheduled for a future time."
            );

        schedulerService.ScheduleJob(
            job,
            request.RunAt,
            ScheduleType.Delayed
        );

        return Results.Accepted(
            $"/jobs/{job.Id}",
            new ScheduleJobResponse(
                job.Id,
                request.RunAt,
                ScheduleType.Delayed
            )
        );
    }
}