using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data;
public interface IAanDataContext
{
    DbSet<Notification> Notifications { get; }
    DbSet<Member> Members { get; }
    DbSet<MemberProfile> MemberProfiles { get; }
    DbSet<MemberPreference> MemberPreferences { get; }
    DbSet<Audit> Audits { get; }
    DbSet<Apprentice> Apprentices { get; }
    DbSet<Employer> Employers { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<CalendarEvent> CalendarEvents { get; }
    DbSet<JobAudit> JobAudits { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
