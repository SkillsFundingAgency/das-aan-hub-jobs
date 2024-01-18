using System;
using System.Linq;
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
            await UpdateMemberProfile(member, cancellationToken);
            await UpdateMemberPreference(member, cancellationToken);
            await UpdateMemberNotifications(member, cancellationToken);
            await UpdateMemberAudit(member, cancellationToken);
            await DeleteMemberUserType(member, cancellationToken);
            await UpdateMemberFutureAttendance(member, cancellationToken);

            await _aanDataContext.SaveChangesAsync(cancellationToken);
        }

        return membersToClean.Count();
    }

    private async Task UpdateMember(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.UpdateMemberDetails(member, cancellationToken);
    }

    private async Task UpdateMemberProfile(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.DeleteMemberProfile(member.MemberProfiles, cancellationToken);
    }

    private async Task UpdateMemberPreference(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.DeleteMemberPreference(member.MemberPreferences, cancellationToken);
    }

    private async Task UpdateMemberNotifications(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.DeleteMemberNotifications(member.Notifications, cancellationToken);
    }

    private async Task UpdateMemberAudit(Member member, CancellationToken cancellationToken)
    {
        await _memberDataCleanupRepository.DeleteMemberAudit(member.Audits, cancellationToken);
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
