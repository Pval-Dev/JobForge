namespace JobForge.Workers;

using System.Collections.Concurrent;

public class WorkerRegistry
{
    private readonly ConcurrentDictionary<Guid, Worker> _workers = new();

    public void RegisterWorker(Worker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);
        if (!_workers.TryAdd(worker.Id, worker)) throw new InvalidOperationException("Worker already exists.");
    }

    public Worker GetWorker(Guid id)
    {
        if (!_workers.TryGetValue(id, out Worker? worker)) throw new KeyNotFoundException("Worker does not exist.");
        return worker;
    }

    public IEnumerable<Worker> GetAllWorkers() => _workers.Values;
}