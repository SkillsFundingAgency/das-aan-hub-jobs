using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetMembers(CancellationToken cancellationToken, Guid[] ids);
        Task UpdateMembers(CancellationToken cancellationToken, List<Member> members);
    }
}
