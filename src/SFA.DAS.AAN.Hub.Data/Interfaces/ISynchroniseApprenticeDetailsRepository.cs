using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface ISynchroniseApprenticeDetailsRepository
    {
        void UpdateMemberDetails(Member member, string firstName, string lastName, string email);
        Task AddJobAudit(JobAudit audit, string? content, CancellationToken cancellationToken);
    }
}
