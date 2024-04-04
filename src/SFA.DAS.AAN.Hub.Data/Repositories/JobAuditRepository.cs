using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class JobAuditRepository : IJobAuditRepository
    {
        private readonly IAanDataContext _context;

        public JobAuditRepository(IAanDataContext context) 
        { 
            _context = context;
        }

        public async Task<JobAudit?> GetMostRecentJobAudit(string jobName, CancellationToken cancellationToken)
        {
            return await _context.JobAudits
                            .Where(a => string.Equals(a.JobName, jobName, StringComparison.Ordinal))
                            .AsNoTracking()
                            .OrderByDescending(a => a.StartTime)
                            .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
