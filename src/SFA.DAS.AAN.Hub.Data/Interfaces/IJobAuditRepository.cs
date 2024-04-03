using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IJobAuditRepository
    {
        Task<JobAudit?> GetMostRecentJobAudit(CancellationToken cancellationToken);

        Task RecordAudit(CancellationToken cancellationToken, JobAudit jobAudit);
    }
}
