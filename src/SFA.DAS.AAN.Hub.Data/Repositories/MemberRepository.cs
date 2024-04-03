using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ILogger<MemberRepository> _logger;
        private readonly IAanDataContext _context;

        public MemberRepository(ILogger<MemberRepository> logger, IAanDataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<Member>> GetMembers(CancellationToken cancellationToken, Guid[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    throw new ArgumentException("ids array must not be null or empty.");
                }

                return await _context.Members.Where(a => ids.Contains(a.Id)).ToListAsync(cancellationToken);
            }
            catch(Exception _exception)
            {
                _logger.LogError(_exception, "Unable to get members.");
                throw;
            }
        }

        public async Task UpdateMembers(CancellationToken cancellationToken, List<Member> members)
        {
            try
            {
                _context.Members.UpdateRange(members);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException _exception)
            {
                _logger.LogError(_exception, "Unable to update members.");
                throw;
            }
        }
    }
}
