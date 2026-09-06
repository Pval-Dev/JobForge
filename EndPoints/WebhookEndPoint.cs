namespace JobForge.EndPoints;

using System.Security.Claims;
using JobForge.Webhooks;

public record RegisterWebhookRequest(string Url, WebhookEvent Event);
public record WebhookResponse(Guid Id, Guid OwnerId, string Url, WebhookEvent Event, bool IsActive, DateTimeOffset CreatedAt);
public record TestWebhookRequest(string Message);

public static class WebhookEndPoint
{
    public static void MapWebhookEndPoints(this WebApplication app)
    {
        var webhooks = app.MapGroup("/webhooks");
        webhooks.MapPost("", RegisterWebhook);
        webhooks.MapGet("", GetMyWebhooks);
        webhooks.MapGet("/all", GetAllWebhooks).RequireAuthorization("AdminOnly");
        webhooks.MapPatch("/{id:guid}/activate", ActivateWebhook);
        webhooks.MapPatch("/{id:guid}/deactivate", DeactivateWebhook);
        webhooks.MapPost("/{id:guid}/test", TestWebhook);
    }

    private static IResult RegisterWebhook(RegisterWebhookRequest request, ClaimsPrincipal principal, WebhookService webhookService)
    {
        Guid ownerId = principal.GetUserId();
        Webhook webhook = webhookService.Register(ownerId, request.Url, request.Event);
        return Results.Created($"/webhooks/{webhook.Id}", ToResponse(webhook));
    }

    private static IResult GetMyWebhooks(ClaimsPrincipal principal, WebhookService webhookService)
    {
        Guid ownerId = principal.GetUserId();
        return Results.Ok(webhookService.GetByOwner(ownerId).Select(ToResponse));
    }

    private static IResult GetAllWebhooks(WebhookService webhookService)
        => Results.Ok(webhookService.GetAll().Select(ToResponse));

    private static IResult ActivateWebhook(Guid id, ClaimsPrincipal principal, WebhookService webhookService)
    {
        Guid userId = principal.GetUserId();
        Webhook webhook = webhookService.GetWebhook(id);
        if (!principal.IsAdmin() && webhook.OwnerId != userId) return Results.Forbid();
        webhookService.Activate(id);
        return Results.NoContent();
    }

    private static IResult DeactivateWebhook(Guid id, ClaimsPrincipal principal, WebhookService webhookService)
    {
        Guid userId = principal.GetUserId();
        Webhook webhook = webhookService.GetWebhook(id);
        if (!principal.IsAdmin() && webhook.OwnerId != userId) return Results.Forbid();
        webhookService.Deactivate(id);
        return Results.NoContent();
    }

    private static async Task<IResult> TestWebhook(Guid id, TestWebhookRequest request, ClaimsPrincipal principal, WebhookService webhookService, CancellationToken cancellationToken)
    {
        Guid userId = principal.GetUserId();
        Webhook webhook = webhookService.GetWebhook(id);
        if (!principal.IsAdmin() && webhook.OwnerId != userId) return Results.Forbid();
        await webhookService.TestAsync(id, request.Message, cancellationToken);
        return Results.NoContent();
    }

    private static WebhookResponse ToResponse(Webhook webhook) => new(
        webhook.Id, webhook.OwnerId, webhook.Url, webhook.Event, webhook.IsActive, webhook.CreatedAt
    );
}