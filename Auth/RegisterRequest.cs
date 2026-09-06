namespace JobForge.Auth;

public record RegisterRequest(
    string Username,
    string Password
);