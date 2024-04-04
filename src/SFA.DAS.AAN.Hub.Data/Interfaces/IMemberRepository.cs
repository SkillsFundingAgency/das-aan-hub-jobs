using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetMembers(Guid[] ids, CancellationToken cancellationToken);
        Task UpdateMembers(List<Member> members, CancellationToken cancellationToken);
    }
}
