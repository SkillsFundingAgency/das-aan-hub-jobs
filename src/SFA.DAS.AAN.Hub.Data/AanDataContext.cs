using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data;

public class AanDataContext : DbContext, IAanDataContext
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    public DbSet<MemberPreference> MemberPreferences => Set<MemberPreference>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Apprentice> Apprentices => Set<Apprentice>();
    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<JobAudit> JobAudits => Set<JobAudit>();
    public DbSet<MemberNotificationEventFormat> MemberNotificationEventFormats => Set<MemberNotificationEventFormat>();
    public DbSet<MemberNotificationLocation> MemberNotificationLocations => Set<MemberNotificationLocation>();

    public AanDataContext(DbContextOptions<AanDataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AanDataContext).Assembly);
    }
}
