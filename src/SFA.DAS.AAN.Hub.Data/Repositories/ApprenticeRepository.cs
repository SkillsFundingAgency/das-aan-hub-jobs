using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class ApprenticeRepository : IApprenticeRepository
    {
        private readonly ILogger<ApprenticeRepository> _logger;
        private readonly IAanDataContext _context;

        public ApprenticeRepository(ILogger<ApprenticeRepository> logger, IAanDataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<Apprentice>> GetApprentices(CancellationToken cancellationToken)
        {
            try
            {
                return await _context.Apprentices.ToListAsync(cancellationToken);
            }
            catch(Exception _exception)
            {
                _logger.LogError(_exception, "Unable to get all apprentices");
                return new List<Apprentice>();
            }
        }

        public async Task<List<Apprentice>> GetApprentices(CancellationToken cancellationToken, Guid[] ids)
        {
            try
            {
                return await _context.Apprentices
                    .AsNoTracking()
                    .Where(a => ids.Contains(a.ApprenticeId))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception _exception)
            {
                _logger.LogError(_exception, "Unable to get apprentices by id array");
                return new List<Apprentice>();
            }
        }
    }
}
