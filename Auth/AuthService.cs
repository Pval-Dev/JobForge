namespace JobForge.Auth;

using System.Security.Authentication;

using JobForge.Persistence;
using JobForge.Users;

public class AuthService
{
    private readonly UserService _userService;
    private readonly UserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        UserService userService,
        UserRepository userRepository,
        JwtTokenService jwtTokenService)
    {
        _userService = userService;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public AuthResponse Register(
        string username,
        string password)
    {
        User user = _userService.CreateUser(
            username,
            password,
            UserRole.User
        );

        return CreateAuthResponse(user);
    }

    public AuthResponse Login(
        string username,
        string password)
    {
        User? user = _userRepository.GetByUsername(username);

        // Use the same error for unknown usernames and invalid passwords
        // to avoid leaking whether an account exists.
        if (user is null)
            throw new AuthenticationException(
                "Invalid username or password."
            );

        bool passwordIsValid =
            _userService.VerifyPassword(user, password);

        if (!passwordIsValid)
            throw new AuthenticationException(
                "Invalid username or password."
            );

        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "User account is inactive."
            );

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = _jwtTokenService.CreateToken(user);

        return new AuthResponse(
            token.Token,
            token.ExpiresAt,
            user.Id,
            user.Username,
            user.Role
        );
    }
}