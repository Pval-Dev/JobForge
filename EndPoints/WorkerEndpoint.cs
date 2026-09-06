namespace JobForge.EndPoints;

using JobForge.Workers;

public record CreateWorkerRequest(string Name);
public record WorkerResponse(Guid Id, string Name, WorkerStatus Status, Guid? CurrentJobId, DateTimeOffset CreatedAt, DateTimeOffset? LastActivity);

public static class WorkerEndPoint
{
    public static void MapWorkerEndPoints(this WebApplication app)
    {
        var workers = app.MapGroup("/workers").RequireAuthorization("AdminOnly");
        workers.MapPost("", CreateWorker);
        workers.MapGet("", GetWorkers);
        workers.MapGet("/{id:guid}", GetWorker);
        workers.MapPatch("/{id:guid}/online", SetOnline);
        workers.MapPatch("/{id:guid}/offline", SetOffline);
    }

    private static IResult CreateWorker(CreateWorkerRequest request, WorkerRegistry registry)
    {
        var worker = new Worker(request.Name);
        registry.RegisterWorker(worker);
        return Results.Created($"/workers/{worker.Id}", ToResponse(worker));
    }

    private static IResult GetWorkers(WorkerRegistry registry)
        => Results.Ok(registry.GetAllWorkers().Select(ToResponse));

    private static IResult GetWorker(Guid id, WorkerRegistry registry)
        => Results.Ok(ToResponse(registry.GetWorker(id)));

    private static IResult SetOnline(Guid id, WorkerRegistry registry)
    {
        Worker worker = registry.GetWorker(id);
        worker.SetOnline();
        return Results.Ok(ToResponse(worker));
    }

    private static IResult SetOffline(Guid id, WorkerRegistry registry)
    {
        Worker worker = registry.GetWorker(id);
        worker.SetOffline();
        return Results.Ok(ToResponse(worker));
    }

    private static WorkerResponse ToResponse(Worker worker) => new(
        worker.Id, worker.Name, worker.Status, worker.CurrentJob?.Id, worker.CreatedAt, worker.LastActivity
    );
}