namespace JobForge.EndPoints;

using JobForge.Audit;


public record AuditResponse(
    Guid Id,
    AuditEventType EventType,
    Guid EntityId,
    Guid? UserId,
    string Message,
    DateTimeOffset OccurredAt
);


public static class AuditEndPoint
{
    public static void MapAuditEndPoints(
        this WebApplication app)
    {
        var audit =
            app.MapGroup("/audit")
                .RequireAuthorization(
                    "AdminOnly"
                );


        audit.MapGet(
            "",
            GetAll
        );


        audit.MapGet(
            "/entity/{id:guid}",
            GetByEntity
        );


        audit.MapGet(
            "/user/{id:guid}",
            GetByUser
        );
    }


    private static IResult GetAll(
        AuditService auditService)
    {
        return Results.Ok(
            auditService
                .GetAll()
                .Select(ToResponse)
        );
    }


    private static IResult GetByEntity(
        Guid id,
        AuditService auditService)
    {
        return Results.Ok(
            auditService
                .GetByEntity(id)
                .Select(ToResponse)
        );
    }


    private static IResult GetByUser(
        Guid id,
        AuditService auditService)
    {
        return Results.Ok(
            auditService
                .GetByUser(id)
                .Select(ToResponse)
        );
    }


    private static AuditResponse ToResponse(
        AuditEntry entry)
    {
        return new AuditResponse(
            entry.Id,
            entry.EventType,
            entry.EntityId,
            entry.UserId,
            entry.Message,
            entry.OccurredAt
        );
    }
}