namespace JobForge.Webhooks;

public class Webhook
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Url { get; private set; }
    public WebhookEvent Event { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Webhook() { Url = null!; }

    public Webhook(Guid ownerId, string url, WebhookEvent webhookEvent)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Webhook URL cannot be empty.");
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Url = url;
        Event = webhookEvent;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}