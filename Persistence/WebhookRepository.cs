namespace JobForge.Persistence;

using JobForge.Webhooks;

public class WebhookRepository
{
    private readonly JobForgeDbContext _context;
    public WebhookRepository(JobForgeDbContext context) => _context = context;
    public void Add(Webhook webhook) { _context.Webhooks.Add(webhook); _context.SaveChanges(); }
    public void Update(Webhook webhook) { _context.Webhooks.Update(webhook); _context.SaveChanges(); }
    public Webhook? GetById(Guid id) => _context.Webhooks.Find(id);
    public IEnumerable<Webhook> GetAll() => _context.Webhooks.ToList();
    public IEnumerable<Webhook> GetByOwner(Guid ownerId) => _context.Webhooks.Where(webhook => webhook.OwnerId == ownerId).ToList();
    public IEnumerable<Webhook> GetMatching(Guid ownerId, WebhookEvent webhookEvent) => _context.Webhooks.Where(webhook => webhook.OwnerId == ownerId && webhook.Event == webhookEvent && webhook.IsActive).ToList();
}