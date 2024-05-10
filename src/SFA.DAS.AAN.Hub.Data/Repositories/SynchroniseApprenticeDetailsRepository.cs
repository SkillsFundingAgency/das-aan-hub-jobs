using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories
{
    public class SynchroniseApprenticeDetailsRepository : ISynchroniseApprenticeDetailsRepository
    {
        private readonly IAanDataContext _aanDataContext;

        public SynchroniseApprenticeDetailsRepository(IAanDataContext aanDataContext)
        {
            _aanDataContext = aanDataContext;
        }

        public void UpdateMemberDetails(Member member, string firstName, string lastName, string email)
        {
            member.FirstName = firstName;
            member.LastName = lastName;
            member.Email = email;
        }

        public async Task AddJobAudit(JobAudit audit, string? content, CancellationToken cancellationToken)
        {
            audit.EndTime = DateTime.UtcNow;
            audit.Notes = content;
            await _aanDataContext.JobAudits.AddAsync(audit, cancellationToken);
        }
    }
}
