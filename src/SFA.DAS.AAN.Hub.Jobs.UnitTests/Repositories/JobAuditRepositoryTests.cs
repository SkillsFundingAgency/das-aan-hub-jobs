using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories
{
    public class JobAuditRepositoryTests
    {
        private DbContextOptions<AanDataContext> _dbContextOptions;
        private CancellationToken cancellationToken = CancellationToken.None;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: nameof(AanDataContext))
                .Options;
        }

        [Test]
        public async Task ThenGetsMostRecentJobAudit()
        {
            JobAudit resultOne = new JobAudit() { JobName = nameof(JobAudit), StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(5) };
            JobAudit resultTwo = new JobAudit() { JobName = nameof(JobAudit), StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddMinutes(5) };

            JobAudit? result;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                await context.JobAudits.AddRangeAsync([resultOne, resultTwo]);
                await context.SaveChangesAsync(cancellationToken);
                var sut = new JobAuditRepository(context);
                result = await sut.GetMostRecentJobAudit(nameof(JobAudit), cancellationToken);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartTime.Date, Is.EqualTo(DateTime.UtcNow.Date));
        }
    }
}
