namespace JobForge.Audit;

public class AuditEntry
{
    public Guid Id { get; private set; }
    public AuditEventType EventType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Message { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEntry()
    {
        Message = null!;
    }

    public AuditEntry(
        AuditEventType eventType,
        Guid entityId,
        Guid? userId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.");

        Id = Guid.NewGuid();
        EventType = eventType;
        EntityId = entityId;
        UserId = userId;
        Message = message;
        OccurredAt = DateTimeOffset.UtcNow;
    }
}