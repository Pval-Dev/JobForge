namespace JobForge.Users;

using JobForge.Common;

public class User
{
    public Guid Id { get; private set; }
    public string NormalizedUsername { get; private set; }
    public string Username { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string PasswordHash { get; private set; }

    private User()
    {
        Username = null!;
        NormalizedUsername = null!;
        PasswordHash = null!;
    }

    public User(string username, UserRole role)
    {
        StringValidator.Validate(username);
        username = username.Trim();
        if (username.Length > 100) throw new ArgumentException("Username is too long.");
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role), "Invalid user role.");
        Id = Guid.NewGuid();
        Username = username;
        NormalizedUsername = username.ToUpperInvariant();
        Role = role;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        PasswordHash = null!;
    }

    internal void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.");
        PasswordHash = passwordHash;
    }

    public void Deactivate()
    {
        if (!IsActive) throw new InvalidOperationException("User is already inactive.");
        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive) throw new InvalidOperationException("User is already active.");
        IsActive = true;
    }

    public void ChangeRole(UserRole newRole)
    {
        if (!Enum.IsDefined(newRole)) throw new ArgumentOutOfRangeException(nameof(newRole), "Invalid user role.");
        if (Role == newRole) throw new InvalidOperationException("User already has this role.");
        Role = newRole;
    }
}