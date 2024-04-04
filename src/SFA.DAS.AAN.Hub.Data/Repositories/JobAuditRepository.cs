using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using System;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class JobAuditRepository : IJobAuditRepository
    {
        private readonly ILogger<JobAuditRepository> _logger;
        private readonly IAanDataContext _context;

        public JobAuditRepository(ILogger<JobAuditRepository> logger, IAanDataContext context) 
        { 
            _logger = logger;
            _context = context;
        }

        public async Task<JobAudit?> GetMostRecentJobAudit(CancellationToken cancellationToken)
        {
            try
            {
                return await _context.JobAudits
                                .AsNoTracking()
                                .OrderByDescending(a => a.StartTime)
                                .FirstOrDefaultAsync(cancellationToken);
            }
            catch(Exception _exception)
            {
                _logger.LogError(_exception, "Unable to get most recent successful audit record.");
                return null;
            }
        }

        public async Task RecordAudit(CancellationToken cancellationToken, JobAudit jobAudit)
        {
            try
            {
                _context.JobAudits.Add(jobAudit);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch(Exception _exception)
            {
                _logger.LogError(_exception, "Failed to record job audit: {JobAuditId}", jobAudit?.Id);
                throw;
            }
        }
    }
}
