using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IJobAuditRepository
    {
        Task<JobAudit?> GetMostRecentJobAudit(string jobName, CancellationToken cancellationToken);
    }
}
