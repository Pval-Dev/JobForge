namespace JobForge.EndPoints;

using JobForge.Metrics;

public static class MetricsEndPoint
{
    public static void MapMetricsEndPoints(this WebApplication app)
    {
        var metrics = app.MapGroup("/metrics")
            .RequireAuthorization("AdminOnly");

        metrics.MapGet("/jobs", GetJobMetrics);
        metrics.MapGet("/workers", GetWorkerMetrics);
    }

    private static IResult GetJobMetrics(
        MetricsService metricsService)
    {
        return Results.Ok(metricsService.GetJobMetrics());
    }

    private static IResult GetWorkerMetrics(
        MetricsService metricsService)
    {
        return Results.Ok(metricsService.GetWorkerMetrics());
    }
}