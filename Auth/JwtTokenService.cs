namespace JobForge.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using JobForge.Users;

public class JwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured."
            );

        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured."
            );

        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured."
            );

        _expirationMinutes = configuration.GetValue<int>(
            "Jwt:ExpirationMinutes"
        );

        if (_expirationMinutes <= 0)
            throw new InvalidOperationException(
                "JWT expiration must be greater than zero."
            );

        if (Encoding.UTF8.GetByteCount(_key) < 32)
            throw new InvalidOperationException(
                "JWT key must contain at least 32 bytes."
            );
    }

    public (string Token, DateTimeOffset ExpiresAt)
        CreateToken(User user)
    {
        DateTimeOffset expiresAt =
            DateTimeOffset.UtcNow.AddMinutes(_expirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_key)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        string serializedToken =
            new JwtSecurityTokenHandler().WriteToken(token);

        return (serializedToken, expiresAt);
    }
}