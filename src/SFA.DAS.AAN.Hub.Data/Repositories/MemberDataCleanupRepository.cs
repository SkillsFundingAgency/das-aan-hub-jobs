using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public interface IMemberDataCleanupRepository
{
    Task<List<Member>> GetWithdrawnOrDeletedMembers();
    Task<List<Member>> GetRemovedMembers();
    Task UpdateMemberDetails(Member member, CancellationToken cancellationToken);
    Task DeleteMemberProfile(List<MemberProfile> memberProfiles, CancellationToken cancellationToken);
    Task DeleteMemberPreference(List<MemberPreference> memberPreferences, CancellationToken cancellationToken);
    Task DeleteMemberNotifications(List<Notification> memberNotifications, CancellationToken cancellationToken);
    Task DeleteMemberAudit(List<Audit> memberAudits, CancellationToken cancellationToken);
    Task DeleteMemberApprentice(Member member, CancellationToken cancellationToken);
    Task DeleteMemberEmployer(Member member, CancellationToken cancellationToken);
    Task UpdateMemberFutureAttendance(Member member, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public class MemberDataCleanupRepository : IMemberDataCleanupRepository
{
    private readonly IAanDataContext _context;

    public MemberDataCleanupRepository(IAanDataContext context)
    {
        _context = context;
    }
    public async Task<List<Member>> GetWithdrawnOrDeletedMembers()
    {
        var statuses = new String[] { "Withdrawn", "Deleted" };
        var query = _context.Members
            .Where(m => statuses.Contains(m.Status) && m.Email != m.Id.ToString() &&
                        m.EndDate < DateTime.Today.AddDays(-14))
            .Include(m => m.MemberPreferences)
            .Include(m => m.MemberProfiles)
            .Include(m => m.Notifications)
            .Include(m => m.Audits);

        return await query.ToListAsync();
    }

    public async Task<List<Member>> GetRemovedMembers()
    {
        var query = _context.Members
                .Where(m => m.Status == "Removed" && m.Email != m.Id.ToString())
                .Include(m => m.MemberPreferences)
                .Include(m => m.MemberProfiles)
                .Include(m => m.Notifications)
                .Include(m => m.Audits);

        return await query.ToListAsync();
    }

    public async Task UpdateMemberDetails(Member member, CancellationToken cancellationToken)
    {
        var existingMember = await _context.Members.Where(m => m.Id == member.Id).FirstAsync(cancellationToken);

        existingMember.FirstName = "Past";
        existingMember.LastName = "Member";
        existingMember.Email = member.Id.ToString();
        existingMember.OrganisationName = "";
        existingMember.IsRegionalChair = false;
        existingMember.LastUpdatedDate = DateTime.UtcNow;
    }

    public async Task DeleteMemberProfile(List<MemberProfile> memberProfiles, CancellationToken cancellationToken)
    {
        _context.MemberProfiles.RemoveRange(memberProfiles);
    }

    public async Task DeleteMemberPreference(List<MemberPreference> memberPreferences, CancellationToken cancellationToken)
    {
        _context.MemberPreferences.RemoveRange(memberPreferences);
    }

    public async Task DeleteMemberNotifications(List<Notification> memberNotifications, CancellationToken cancellationToken)
    {
        _context.Notifications.RemoveRange(memberNotifications);
    }

    public async Task DeleteMemberAudit(List<Audit> memberAudits, CancellationToken cancellationToken)
    {
        var auditsToRemove = memberAudits.Select(a => a).Where(x => x.Resource != "Member").ToList();
        _context.Audits.RemoveRange(auditsToRemove);
    }

    public async Task DeleteMemberApprentice(Member member, CancellationToken cancellationToken)
    {
        var apprentices = await _context.Apprentices.Where(a => a.MemberId == member.Id).FirstAsync(cancellationToken);

        _context.Apprentices.RemoveRange(apprentices);
    }

    public async Task DeleteMemberEmployer(Member member, CancellationToken cancellationToken)
    {
        var employers = await _context.Employers.Where(a => a.MemberId == member.Id).FirstAsync(cancellationToken);

        _context.Employers.RemoveRange(employers);
    }

    public async Task UpdateMemberFutureAttendance(Member member, CancellationToken cancellationToken)
    {
        var attendances = await _context.Attendances
            .Where(a => a.MemberId == member.Id)
            .ToListAsync(cancellationToken);

        var attendanceEventIds = attendances.Select(a => a.CalendarEventId);

        var futureCalendarEvents = await _context.CalendarEvents
            .Where(c => attendanceEventIds.Contains(c.Id) && c.StartDate > DateTime.Today)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        foreach (Attendance attendance in attendances)
        {
            if (futureCalendarEvents.Contains(attendance.CalendarEventId))
                attendance.IsAttending = false;
        }
    }
}
