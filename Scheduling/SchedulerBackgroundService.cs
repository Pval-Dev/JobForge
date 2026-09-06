namespace JobForge.Schedule;

public sealed class SchedulerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulerBackgroundService> _logger;

    public SchedulerBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SchedulerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<SchedulerService>();
                    scheduler.ProcessDueJobs(DateTimeOffset.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing scheduled jobs.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during graceful shutdown.
        }
    }
}