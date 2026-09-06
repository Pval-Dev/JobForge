namespace JobForge.Workers;

using JobForge.Job;

public static class JobHandlerRegistration
{
    public static IServiceCollection AddJobForgeHandlers(this IServiceCollection services)
    {
        services.AddSingleton(new JobHandler(
            "ECHO",
            job => new JobResult(true, $"Processed payload: {job.Payload}")
        ));

        services.AddSingleton(new JobHandler(
            "FAIL_ALWAYS",
            _ => new JobResult(false, "Intentional handler failure.")
        ));

        return services;
    }
}