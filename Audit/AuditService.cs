namespace JobForge.Audit;

using JobForge.Persistence;

public class AuditService
{
    private readonly AuditRepository _auditRepository;

    public AuditService(AuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public void Record(
        AuditEventType eventType,
        Guid entityId,
        Guid? userId,
        string message)
    {
        var record = new AuditEntry(
            eventType,
            entityId,
            userId,
            message
        );

        _auditRepository.Add(record);
    }

    public IEnumerable<AuditEntry> GetAll()
    {
        return _auditRepository.GetAll();
    }

    public IEnumerable<AuditEntry> GetByEntity(Guid id)
    {
        return _auditRepository.GetByEntity(id);
    }

    public IEnumerable<AuditEntry> GetByUser(Guid id)
    {
        return _auditRepository.GetByUser(id);
    }
}