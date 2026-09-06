namespace JobForge.Metrics;

public class WorkerMetrics
{
    public int TotalWorkers { get; }
    public int IdleWorkers { get; }
    public int BusyWorkers { get; }
    public int OfflineWorkers { get; }
    public int AvailableWorkers { get; }
    public double UtilizationRate { get; }

    public WorkerMetrics(int totalWorkers, int idleWorkers, int busyWorkers, int offlineWorkers)
    {
        TotalWorkers = totalWorkers;
        IdleWorkers = idleWorkers;
        BusyWorkers = busyWorkers;
        OfflineWorkers = offlineWorkers;
        AvailableWorkers = idleWorkers;
        UtilizationRate = totalWorkers == 0 ? 0 : (double)busyWorkers / totalWorkers * 100;
    }
}