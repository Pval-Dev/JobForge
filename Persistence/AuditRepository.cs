namespace JobForge.Persistence;

using JobForge.Audit;

public class AuditRepository
{
    private readonly JobForgeDbContext _context;
    public AuditRepository(JobForgeDbContext context) => _context = context;
    public void Add(AuditEntry record) { _context.AuditEntries.Add(record); _context.SaveChanges(); }
    public IEnumerable<AuditEntry> GetAll() => _context.AuditEntries.ToList();
    public IEnumerable<AuditEntry> GetByEntity(Guid id) => _context.AuditEntries.Where(entry => entry.EntityId == id).ToList();
    public IEnumerable<AuditEntry> GetByUser(Guid id) => _context.AuditEntries.Where(entry => entry.UserId == id).ToList();
}