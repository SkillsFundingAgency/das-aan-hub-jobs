﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

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
            .Where(m => (m.Status == MemberStatus.Withdrawn || m.Status == MemberStatus.Deleted)
                && m.Email != m.Id.ToString()
                && EF.Functions.DateDiffDay(m.EndDate, DateTime.Today) > 14)
            .Include(m => m.MemberPreferences)
            .Include(m => m.MemberProfiles)
            .Include(m => m.Notifications)
            .Include(m => m.Audits)
            .Include(m => m.Apprentice)
            .Include(m => m.Employer);

        return await query.ToListAsync();
    }

    public async Task<List<Member>> GetRemovedMembers()
    {
        var query = _context.Members
                .Where(m => m.Status == MemberStatus.Removed && m.Email != m.Id.ToString())
                .Include(m => m.MemberPreferences)
                .Include(m => m.MemberProfiles)
                .Include(m => m.Notifications)
                .Include(m => m.Audits)
                .Include(m => m.Apprentice)
                .Include(m => m.Employer);

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

    public void DeleteMemberApprentice(Apprentice memberApprentice, CancellationToken cancellationToken)
    {
        _context.Apprentices.RemoveRange(memberApprentice);
    }

    public void DeleteMemberEmployer(Employer memberEmployer, CancellationToken cancellationToken)
    {
        _context.Employers.RemoveRange(memberEmployer);
    }

    public async Task<List<Attendance>> GetMemberAttendances(Guid memberId, CancellationToken cancellationToken)
    {
        return await _context.Attendances
            .Where(a => a.MemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CalendarEvent>> GetFutureCalendarEvents(List<Guid> attendanceEventIds, CancellationToken cancellationToken)
    {
        return await _context.CalendarEvents
            .Where(c => attendanceEventIds.Contains(c.Id))
            .Select(c => c)
            .ToListAsync(cancellationToken);
    }

    public void UpdateMemberFutureAttendance(Attendance memberAttendance, CancellationToken cancellationToken)
    {
        memberAttendance.IsAttending = false;
    }
}
