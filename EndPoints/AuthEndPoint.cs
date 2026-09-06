namespace JobForge.EndPoints;

using JobForge.Auth;


public static class AuthEndPoint
{
    public static void MapAuthEndPoints(
        this WebApplication app)
    {
        var auth =
            app.MapGroup("/auth")
                .AllowAnonymous();

        auth.MapPost(
            "/register",
            Register
        );

        auth.MapPost(
            "/login",
            Login
        );
    }


    private static IResult Register(
        RegisterRequest request,
        AuthService authService)
    {
        AuthResponse response =
            authService.Register(
                request.Username,
                request.Password
            );

        return Results.Created(
            $"/users/{response.UserId}",
            response
        );
    }


    private static IResult Login(
        LoginRequest request,
        AuthService authService)
    {
        AuthResponse response =
            authService.Login(
                request.Username,
                request.Password
            );

        return Results.Ok(response);
    }
}