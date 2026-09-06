namespace JobForge.Auth;

using JobForge.Users;

public record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Username,
    UserRole Role
);