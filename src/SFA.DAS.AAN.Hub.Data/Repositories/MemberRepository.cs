using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly IAanDataContext _context;

        public MemberRepository(IAanDataContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetActiveApprenticeMembers(CancellationToken cancellationToken)
        {
            return await _context.Members.Where(m => 
                   m.Email != m.Id.ToString() &&
                   m.UserType == UserType.Apprentice
            )
            .Include(a => a.Apprentice)
            .ToListAsync(cancellationToken);
        }
    }
}
