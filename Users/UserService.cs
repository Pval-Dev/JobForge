namespace JobForge.Users;

using Microsoft.AspNetCore.Identity;
using JobForge.Job;
using JobForge.Audit;
using JobForge.Persistence;

public class UserService
{
    private readonly UserRepository _repository;
    private readonly AuditService _auditService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AuditService auditService, UserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _auditService = auditService;
        _repository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public User CreateUser(string username, string password, UserRole role = UserRole.User)
    {
        ValidatePassword(password);
        if (_repository.GetByUsername(username) is not null) throw new InvalidOperationException("Username already exists.");
        var user = new User(username, role);
        string passwordHash = _passwordHasher.HashPassword(user, password);
        user.SetPasswordHash(passwordHash);
        _repository.Add(user);
        _auditService.Record(AuditEventType.UserCreated, user.Id, null, $"User '{user.Username}' was created.");
        return user;
    }

    public User? GetUser(Guid id) => _repository.GetById(id);

    private User GetUserOrThrow(Guid id) => GetUser(id) ?? throw new KeyNotFoundException("User does not exist.");

    public bool VerifyPassword(User user, string password)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(user.PasswordHash)) return false;
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            string newHash = _passwordHasher.HashPassword(user, password);
            user.SetPasswordHash(newHash);
            _repository.Update(user);
        }
        return result != PasswordVerificationResult.Failed;
    }

    public void DeactivateUser(Guid id)
    {
        User user = GetUserOrThrow(id);
        user.Deactivate();
        _repository.Update(user);
        _auditService.Record(AuditEventType.UserDeactivated, user.Id, null, $"User '{user.Username}' was deactivated.");
    }

    public void ActivateUser(Guid id)
    {
        User user = GetUserOrThrow(id);
        user.Activate();
        _repository.Update(user);
        _auditService.Record(AuditEventType.UserActivated, user.Id, null, $"User '{user.Username}' was activated.");
    }

    public void ChangeRole(Guid id, UserRole newRole)
    {
        User user = GetUserOrThrow(id);
        UserRole oldRole = user.Role;
        user.ChangeRole(newRole);
        _repository.Update(user);
        _auditService.Record(AuditEventType.UserRoleChanged, user.Id, null, $"User '{user.Username}' role changed from {oldRole} to {newRole}.");
    }

    public bool CanAccessJob(Guid userId, Job job) => IsAdminOrOwner(userId, job);
    public bool CanCancelJob(Guid userId, Job job) => IsAdminOrOwner(userId, job);

    private bool IsAdminOrOwner(Guid userId, Job job)
    {
        User user = GetUserOrThrow(userId);
        return user.Role == UserRole.Admin || user.Id == job.OwnerId;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty.");
        if (password.Length < 8) throw new ArgumentException("Password must contain at least 8 characters.");
        if (password.Length > 256) throw new ArgumentException("Password is too long.");
    }
}