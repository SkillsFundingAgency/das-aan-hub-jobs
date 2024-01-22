using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public interface IMemberDataCleanupRepository
{
    Task<List<Member>> GetWithdrawnOrDeletedMembers();
    Task<List<Member>> GetRemovedMembers();
    void UpdateMemberDetails(Member member, CancellationToken cancellationToken);
    void DeleteMemberProfile(List<MemberProfile> memberProfiles, CancellationToken cancellationToken);
    void DeleteMemberPreference(List<MemberPreference> memberPreferences, CancellationToken cancellationToken);
    void DeleteMemberNotifications(List<Notification> memberNotifications, CancellationToken cancellationToken);
    void DeleteMemberAudit(List<Audit> memberAudits, CancellationToken cancellationToken);
    void DeleteMemberApprentice(List<Apprentice> memberApprentices, CancellationToken cancellationToken);
    void DeleteMemberEmployer(List<Employer> memberEmployers, CancellationToken cancellationToken);
    Task<List<Attendance>> GetMemberAttendances(Guid memberId, CancellationToken cancellationToken);
    Task<List<Guid>> GetFutureCalendarEvents(List<Guid> attendanceEventIds, CancellationToken cancellationToken);
    void UpdateMemberFutureAttendance(Attendance memberAttendance, CancellationToken cancellationToken);
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
        var query = _context.Members
            .Where(m => m.Status == MemberStatus.Withdrawn.ToString() || m.Status == MemberStatus.Deleted.ToString()
                && m.Email != m.Id.ToString()
                && m.EndDate.Value.Day < DateTime.Today.AddDays(-14).Day).
            Include(m => m.MemberPreferences)
            .Include(m => m.MemberProfiles)
            .Include(m => m.Notifications)
            .Include(m => m.Audits);

        return await query.ToListAsync();
    }

    public async Task<List<Member>> GetRemovedMembers()
    {
        var query = _context.Members
                .Where(m => m.Status == MemberStatus.Removed.ToString() && m.Email != m.Id.ToString())
                .Include(m => m.MemberPreferences)
                .Include(m => m.MemberProfiles)
                .Include(m => m.Notifications)
                .Include(m => m.Audits)
                .Include(m => m.Apprentices)
                .Include(m => m.Employers);

        return await query.ToListAsync();
    }

    public void UpdateMemberDetails(Member member, CancellationToken cancellationToken)
    {
        member.FirstName = "Past";
        member.LastName = "Member";
        member.Email = member.Id.ToString();
        member.OrganisationName = "";
        member.IsRegionalChair = false;
        member.LastUpdatedDate = DateTime.UtcNow;
    }

    public void DeleteMemberProfile(List<MemberProfile> memberProfiles, CancellationToken cancellationToken)
    {
        _context.MemberProfiles.RemoveRange(memberProfiles);
    }

    public void DeleteMemberPreference(List<MemberPreference> memberPreferences, CancellationToken cancellationToken)
    {
        _context.MemberPreferences.RemoveRange(memberPreferences);
    }

    public void DeleteMemberNotifications(List<Notification> memberNotifications, CancellationToken cancellationToken)
    {
        _context.Notifications.RemoveRange(memberNotifications);
    }

    public void DeleteMemberAudit(List<Audit> memberAudits, CancellationToken cancellationToken)
    {
        _context.Audits.RemoveRange(memberAudits);
    }

    public void DeleteMemberApprentice(List<Apprentice> memberApprentices, CancellationToken cancellationToken)
    {
        _context.Apprentices.RemoveRange(memberApprentices);
    }

    public void DeleteMemberEmployer(List<Employer> memberEmployers, CancellationToken cancellationToken)
    {
        _context.Employers.RemoveRange(memberEmployers);
    }

    public async Task<List<Attendance>> GetMemberAttendances(Guid memberId, CancellationToken cancellationToken)
    {
        return await _context.Attendances
            .Where(a => a.MemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetFutureCalendarEvents(List<Guid> attendanceEventIds, CancellationToken cancellationToken)
    {
        return await _context.CalendarEvents
            .Where(c => attendanceEventIds.Contains(c.Id) && c.StartDate > DateTime.Today)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public void UpdateMemberFutureAttendance(Attendance memberAttendance, CancellationToken cancellationToken)
    {
        memberAttendance.IsAttending = false;
    }
}
