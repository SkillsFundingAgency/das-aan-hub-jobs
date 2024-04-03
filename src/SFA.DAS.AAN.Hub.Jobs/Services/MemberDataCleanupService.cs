using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IMemberDataCleanupService
{
    Task<int> ProcessMemberDataCleanup(CancellationToken cancellationToken);
}

public class MemberDataCleanupService : IMemberDataCleanupService
{
    private readonly IAanDataContext _aanDataContext;
    private readonly IMemberDataCleanupRepository _memberDataCleanupRepository;

    public MemberDataCleanupService(IAanDataContext aanDataContext, IMemberDataCleanupRepository memberDataCleanupRepository)
    {
        _aanDataContext = aanDataContext;
        _memberDataCleanupRepository = memberDataCleanupRepository;
    }

    public async Task<int> ProcessMemberDataCleanup(CancellationToken cancellationToken)
    {
        var membersToClean = await _memberDataCleanupRepository.GetWithdrawnOrDeletedMembers();
        var removedMembers = await _memberDataCleanupRepository.GetRemovedMembers();
        membersToClean.AddRange(removedMembers);

        foreach (Member member in membersToClean)
        {
            UpdateMember(member, cancellationToken);
            UpdateMemberProfile(member, cancellationToken);
            UpdateMemberPreference(member, cancellationToken);
            UpdateMemberNotifications(member, cancellationToken);
            UpdateMemberAudit(member, cancellationToken);
            DeleteMemberUserType(member, cancellationToken);
            await UpdateMemberFutureAttendance(member, cancellationToken);

            await _aanDataContext.SaveChangesAsync(cancellationToken);
        }

        return membersToClean.Count;
    }

    private void UpdateMember(Member member, CancellationToken cancellationToken)
    {
        _memberDataCleanupRepository.UpdateMemberDetails(member, cancellationToken);
    }

    private void UpdateMemberProfile(Member member, CancellationToken cancellationToken)
    {
        _memberDataCleanupRepository.DeleteMemberProfile(member.MemberProfiles, cancellationToken);
    }

    private void UpdateMemberPreference(Member member, CancellationToken cancellationToken)
    {
        _memberDataCleanupRepository.DeleteMemberPreference(member.MemberPreferences, cancellationToken);
    }

    private void UpdateMemberNotifications(Member member, CancellationToken cancellationToken)
    {
        _memberDataCleanupRepository.DeleteMemberNotifications(member.Notifications, cancellationToken);
    }

    private void UpdateMemberAudit(Member member, CancellationToken cancellationToken)
    {
        var auditsToRemove = member.Audits.Select(a => a).Where(x => x.Resource != "Member").ToList();

        if (auditsToRemove.Count > 0)
            _memberDataCleanupRepository.DeleteMemberAudit(auditsToRemove, cancellationToken);
    }

    private void DeleteMemberUserType(Member member, CancellationToken cancellationToken)
    {
        if (member.UserType == UserType.Apprentice)
        {
            _memberDataCleanupRepository.DeleteMemberApprentice(member.Apprentice!, cancellationToken);
        }
        else if (member.UserType == UserType.Employer)
        {
            _memberDataCleanupRepository.DeleteMemberEmployer(member.Employer!, cancellationToken);
        }
    }

    private async Task UpdateMemberFutureAttendance(Member member, CancellationToken cancellationToken)
    {
        var attendances = await _memberDataCleanupRepository.GetMemberAttendances(member.Id, cancellationToken);

        var attendanceEventIds = attendances.Select(a => a.CalendarEventId).ToList();

        if (attendanceEventIds.Count > 0)
        {
            var futureCalendarEvents =
                _memberDataCleanupRepository.GetFutureCalendarEvents(attendanceEventIds, cancellationToken);

            var futureEventIds = futureCalendarEvents.Result
                .Where(e => e.StartDate > DateTime.Today)
                .Select(e => e.Id)
                .ToList();

            foreach (Attendance attendance in attendances.Where(a => futureEventIds.Contains(a.CalendarEventId)))
            {
                _memberDataCleanupRepository.UpdateMemberFutureAttendance(attendance, cancellationToken);
            }
        }
    }
}
