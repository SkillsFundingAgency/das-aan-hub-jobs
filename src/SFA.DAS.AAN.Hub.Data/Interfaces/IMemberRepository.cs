using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetActiveMembers(CancellationToken cancellationToken);
    }
}
