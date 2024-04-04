using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories
{
    public class JobAuditRepositoryTests
    {
        private DbContextOptions<AanDataContext> _dbContextOptions;
        private Mock<ILogger<JobAuditRepository>> _logger;
        private CancellationToken cancellationToken = CancellationToken.None;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: nameof(AanDataContext))
                .Options;

            _logger = new Mock<ILogger<JobAuditRepository>>();
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
                var sut = new JobAuditRepository(_logger.Object, context);
                result = await sut.GetMostRecentJobAudit(cancellationToken);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartTime.Date, Is.EqualTo(DateTime.UtcNow.Date));
        }

        [Test]
        public async Task ThenRecordAuditIsSuccessful()
        {
            JobAudit audit = new JobAudit() { JobName = "RecordedAudit", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow };

            JobAudit? recordedAudit;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                var sut = new JobAuditRepository(_logger.Object, context);
                await sut.RecordAudit(audit, cancellationToken);
                recordedAudit = context.JobAudits.FirstOrDefault(t => t.JobName == "RecordedAudit");
            }

            Assert.That(recordedAudit, Is.Not.Null);
        }
    }
}
