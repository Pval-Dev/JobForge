namespace JobForge.Webhooks;

using System.Net.Http.Json;
using JobForge.Users;
using JobForge.Persistence;

public record WebhookDelivery(WebhookEvent Event, string Message, DateTimeOffset OccurredAt);

public class WebhookService
{
    private readonly WebhookRepository _repository;
    private readonly UserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(UserService userService, WebhookRepository repository, IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
    {
        _userService = userService;
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Webhook Register(Guid ownerId, string url, WebhookEvent webhookEvent)
    {
        var user = _userService.GetUser(ownerId) ?? throw new KeyNotFoundException("User does not exist.");
        if (!user.IsActive) throw new InvalidOperationException("User is inactive.");
        ValidateUrl(url);
        var webhook = new Webhook(ownerId, url, webhookEvent);
        _repository.Add(webhook);
        return webhook;
    }

    public Webhook GetWebhook(Guid id) => GetWebhookOrThrow(id);
    public IEnumerable<Webhook> GetByOwner(Guid ownerId) => _repository.GetByOwner(ownerId);
    public IEnumerable<Webhook> GetAll() => _repository.GetAll();

    public void Deactivate(Guid webhookId)
    {
        var webhook = GetWebhookOrThrow(webhookId);
        webhook.Deactivate();
        _repository.Update(webhook);
    }

    public void Activate(Guid webhookId)
    {
        var webhook = GetWebhookOrThrow(webhookId);
        webhook.Activate();
        _repository.Update(webhook);
    }

    public async Task NotifyAsync(Guid ownerId, WebhookEvent webhookEvent, string message, CancellationToken cancellationToken = default)
    {
        foreach (Webhook webhook in _repository.GetMatching(ownerId, webhookEvent))
        {
            try { await SendAsync(webhook, message, cancellationToken); }
            catch (Exception ex) { _logger.LogError(ex, "Webhook delivery failed for {WebhookId} to {Url}.", webhook.Id, webhook.Url); }
        }
    }

    public async Task TestAsync(Guid webhookId, string message, CancellationToken cancellationToken = default)
    {
        Webhook webhook = GetWebhookOrThrow(webhookId);
        if (!webhook.IsActive) throw new InvalidOperationException("Webhook is inactive.");
        await SendAsync(webhook, message, cancellationToken);
    }

    private async Task SendAsync(Webhook webhook, string message, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("Webhooks");
        var payload = new WebhookDelivery(webhook.Event, message, DateTimeOffset.UtcNow);
        using HttpResponseMessage response = await client.PostAsJsonAsync(webhook.Url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Webhook {WebhookId} delivered to {Url}.", webhook.Id, webhook.Url);
    }

    private Webhook GetWebhookOrThrow(Guid id) => _repository.GetById(id) ?? throw new KeyNotFoundException("Webhook does not exist.");

    private static void ValidateUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) throw new ArgumentException("Webhook URL is invalid.");
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Webhook URL must use HTTP or HTTPS.");
    }
}