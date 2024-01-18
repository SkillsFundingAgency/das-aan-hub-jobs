using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;

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
            await UpdateMember(member, cancellationToken);
            UpdateMemberProfile(member, cancellationToken);
            UpdateMemberPreference(member, cancellationToken);
            UpdateMemberNotifications(member, cancellationToken);
            UpdateMemberAudit(member, cancellationToken);
            await DeleteMemberUserType(member, cancellationToken);
            await UpdateMemberFutureAttendance(member, cancellationToken);

            await _aanDataContext.SaveChangesAsync(cancellationToken);
        }

        return membersToClean.Count;
    }

    private async Task UpdateMember(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.UpdateMemberDetails(member, cancellationToken);
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
        _memberDataCleanupRepository.DeleteMemberAudit(member.Audits, cancellationToken);
    }

    private async Task DeleteMemberUserType(Member member, CancellationToken cancellationToken)
    {
        if (string.Equals(member.UserType.ToString(), "apprentice", StringComparison.CurrentCultureIgnoreCase))
        {
            await _memberDataCleanupRepository.DeleteMemberApprentice(member, cancellationToken);
        }
        else if (string.Equals(member.UserType.ToString(), "employer", StringComparison.CurrentCultureIgnoreCase))
        {
            await _memberDataCleanupRepository.DeleteMemberEmployer(member, cancellationToken);
        }
    }

    private async Task UpdateMemberFutureAttendance(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.UpdateMemberFutureAttendance(member, cancellationToken);
    }
}
