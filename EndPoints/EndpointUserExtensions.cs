namespace JobForge.EndPoints;

using System.Security.Claims;

using JobForge.Users;


public static class EndpointUserExtensions
{
    public static Guid GetUserId(
        this ClaimsPrincipal principal)
    {
        string? rawId =
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!Guid.TryParse(rawId, out Guid userId))
            throw new UnauthorizedAccessException(
                "Authenticated user id is invalid."
            );

        return userId;
    }


    public static bool IsAdmin(
        this ClaimsPrincipal principal)
    {
        return principal.IsInRole(
            UserRole.Admin.ToString()
        );
    }
}