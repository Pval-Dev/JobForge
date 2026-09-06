namespace JobForge.Workers;

public class WorkerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerRegistry _workerRegistry;

    public WorkerBackgroundService(IServiceScopeFactory scopeFactory, WorkerRegistry workerRegistry)
    {
        _scopeFactory = scopeFactory;
        _workerRegistry = workerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var idleWorkers = _workerRegistry.GetAllWorkers().Where(worker => worker.Status == WorkerStatus.Idle).ToList();
            if (idleWorkers.Count == 0)
            {
                await Task.Delay(300, stoppingToken);
                continue;
            }

            var tasks = idleWorkers.Select(worker => ProcessWorkerAsync(worker, stoppingToken));
            await Task.WhenAll(tasks);
            await Task.Delay(300, stoppingToken);
        }
    }

    private async Task ProcessWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<JobProcessor>();
        await processor.ProcessNextJobAsync(worker, cancellationToken);
    }
}