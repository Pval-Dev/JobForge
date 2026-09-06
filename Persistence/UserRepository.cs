namespace JobForge.Persistence;

using JobForge.Users;

public class UserRepository
{
    private readonly JobForgeDbContext _context;
    public UserRepository(JobForgeDbContext context) => _context = context;
    public void Add(User user) { _context.Users.Add(user); _context.SaveChanges(); }
    public void Update(User user) { _context.Users.Update(user); _context.SaveChanges(); }
    public User? GetById(Guid id) => _context.Users.Find(id);
    public User? GetByUsername(string username)
    {
        string normalizedUsername = username.Trim().ToUpperInvariant();
        return _context.Users.SingleOrDefault(user => user.NormalizedUsername == normalizedUsername);
    }
    public IEnumerable<User> GetAll() => _context.Users.ToList();
    public bool AnyAdmin() => _context.Users.Any(user => user.Role == UserRole.Admin);
}