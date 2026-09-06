namespace JobForge.Persistence;

using JobForge.Schedule;

public class ScheduleRepository
{
    private readonly JobForgeDbContext _context;
    public ScheduleRepository(JobForgeDbContext context) => _context = context;
    public void Add(Schedule schedule) { _context.Schedules.Add(schedule); _context.SaveChanges(); }
    public void Update(Schedule schedule) { _context.Schedules.Update(schedule); _context.SaveChanges(); }
    public Schedule? GetById(Guid id) => _context.Schedules.Find(id);
    public IEnumerable<Schedule> GetAll() => _context.Schedules.ToList();
    public IEnumerable<Schedule> GetDue(DateTimeOffset now) => _context.Schedules.Where(schedule => schedule.IsActive && schedule.RunAt <= now).ToList();
}