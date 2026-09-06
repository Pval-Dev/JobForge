namespace JobForge.Auth;

public record LoginRequest(
    string Username,
    string Password
);