namespace JobForge.EndPoints;

using System.Security.Claims;
using JobForge.Users;

public record ChangeUserRoleRequest(UserRole Role);

public record UserResponse(
    Guid Id,
    string Username,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public static class UserEndPoint
{
    public static void MapUserEndPoints(this WebApplication app)
    {
        var users = app.MapGroup("/users");
        users.MapGet("/me", GetCurrentUser);
        users.MapGet("/{id:guid}", GetUser).RequireAuthorization("AdminOnly");
        users.MapPatch("/{id:guid}/activate", ActivateUser).RequireAuthorization("AdminOnly");
        users.MapPatch("/{id:guid}/deactivate", DeactivateUser).RequireAuthorization("AdminOnly");
        users.MapPatch("/{id:guid}/role", ChangeRole).RequireAuthorization("AdminOnly");
    }

    private static IResult GetCurrentUser(ClaimsPrincipal principal, UserService userService)
    {
        Guid userId = principal.GetUserId();
        User? user = userService.GetUser(userId);
        return user is null ? Results.NotFound() : Results.Ok(ToResponse(user));
    }

    private static IResult GetUser(Guid id, UserService userService)
    {
        User? user = userService.GetUser(id);
        return user is null ? Results.NotFound() : Results.Ok(ToResponse(user));
    }

    private static IResult ActivateUser(Guid id, UserService userService)
    {
        userService.ActivateUser(id);
        return Results.NoContent();
    }

    private static IResult DeactivateUser(Guid id, UserService userService)
    {
        userService.DeactivateUser(id);
        return Results.NoContent();
    }

    private static IResult ChangeRole(Guid id, ChangeUserRoleRequest request, UserService userService)
    {
        userService.ChangeRole(id, request.Role);
        return Results.NoContent();
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Username,
        user.Role,
        user.IsActive,
        user.CreatedAt
    );
}