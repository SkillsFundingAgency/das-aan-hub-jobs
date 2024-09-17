using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetActiveApprenticeMembers(CancellationToken cancellationToken);
        Task<MemberDetails> GetAdminMemberEmailById(Guid id, CancellationToken cancellationToken);
    }
}
