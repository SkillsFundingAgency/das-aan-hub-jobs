using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.Testing.AutoFixture;
using System.Threading;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories
{
    public class SynchroniseApprenticeDetailsRepositoryTest
    {
        private readonly Mock<IAanDataContext> aanDataContextMock = null!;
        private DbContextOptions<AanDataContext> _dbContextOptions;
        private CancellationToken cancellationToken = CancellationToken.None;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task ThenUpdateMemberDetails()
        {
            var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                    .CreateMany(1)
                                    .ToList();

            _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: nameof(ThenUpdateMemberDetails))
                .Options;

            Member updatedMember = null!;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                await context.Members.AddRangeAsync(membersToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var memberToUpdate = await context.Members.FirstAsync(a => a.Id == membersToAdd[0].Id);

                var sut = new SynchroniseApprenticeDetailsRepository(context);
                sut.UpdateMemberDetails(memberToUpdate, "test", "test", "test");

                await context.SaveChangesAsync(cancellationToken);

                updatedMember = await context.Members.FirstAsync(a => a.Id == membersToAdd[0].Id);
            }

            Assert.That(updatedMember, Is.Not.Null);
            Assert.That(updatedMember.FirstName, Is.EqualTo("test"));
            Assert.That(updatedMember.LastName, Is.EqualTo("test"));
            Assert.That(updatedMember.Email, Is.EqualTo("test"));
        }

        [Test]
        public async Task ThenAddJobAudit()
        {
            var startTime = DateTime.UtcNow;

            var JobAudit = new JobAudit() { 
                JobName = nameof(ThenAddJobAudit), 
                StartTime = startTime
            };

            _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: nameof(ThenUpdateMemberDetails))
                .Options;

            JobAudit jobAuditAdded = null!;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                var sut = new SynchroniseApprenticeDetailsRepository(context);
                await sut.AddJobAudit(JobAudit, "content", cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                jobAuditAdded = await context.JobAudits.FirstAsync(a => a.JobName == nameof(ThenAddJobAudit));
            }

            Assert.That(jobAuditAdded, Is.Not.Null);
            Assert.That(jobAuditAdded.StartTime, Is.EqualTo(startTime));
            Assert.That(jobAuditAdded.EndTime, Is.AtLeast(startTime));
            Assert.That(jobAuditAdded.Notes, Is.EqualTo("content"));
        }
    }
}
