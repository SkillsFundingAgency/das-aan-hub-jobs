using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.JobAudits
{
    public class JobAuditForSynchronisedApprentice
    {
        private CancellationToken cancellationToken = CancellationToken.None;
        private Fixture _fixture = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task When_synchronising_apprentices_add_job_audit()
        {
            var startTime = DateTime.UtcNow;

            var jobAudit = new JobAudit()
            {
                JobName = "JobName",
                StartTime = startTime
            };

            JobAudit jobAuditAdded = null!;

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(When_synchronising_apprentices_add_job_audit)}_InMemoryContext"))
            {
                var sut = new SynchroniseApprenticeDetailsRepository(context);
                await sut.AddJobAudit(jobAudit, "content", cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                jobAuditAdded = await context.JobAudits.FirstAsync(a => a.JobName == "JobName");
            }

            Assert.Multiple(() =>
            {
                Assert.That(jobAuditAdded, Is.Not.Null);
                Assert.That(jobAuditAdded.StartTime, Is.EqualTo(startTime));
                Assert.That(jobAuditAdded.EndTime, Is.AtLeast(startTime));
                Assert.That(jobAuditAdded.Notes, Is.EqualTo("content"));
            });
        }
    }
}
