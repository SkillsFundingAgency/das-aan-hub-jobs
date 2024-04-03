using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.FixtureSpecimens;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories
{
    public class ApprenticeRepositoryTests
    {
        private DbContextOptions<AanDataContext> _dbContextOptions;
        private Mock<ILogger<ApprenticeRepository>> _logger;
        private CancellationToken cancellationToken = CancellationToken.None;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customizations.Add(new GuidSpecimenBuilder());

            _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: nameof(AanDataContext))
                .Options;

            _logger = new Mock<ILogger<ApprenticeRepository>>();
        }

        [Test]
        public async Task ThenGetApprentices()
        {
            var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                        .CreateMany(3)
                                        .ToList();

            var apprenticesToAdd = _fixture.Build<Apprentice>()
                                        .Without(m => m.Member)
                                        .CreateMany(3)
                                        .ToList();

            for (int i = 0; i < 3; i++)
            {
                apprenticesToAdd[i].MemberId = membersToAdd[i].Id;
            }

            List<Apprentice> result;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                await context.Apprentices.AddRangeAsync(apprenticesToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var sut = new ApprenticeRepository(_logger.Object, context);
                result = await sut.GetApprentices(cancellationToken);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task ThenGetApprenticesById()
        {
            var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                        .CreateMany(3)
                                        .ToList();

            var apprenticesToAdd = _fixture.Build<Apprentice>()
                                        .Without(m => m.Member)
                                        .CreateMany(3)
                                        .ToList();

            for (int i = 0; i < 3; i++)
            {
                apprenticesToAdd[i].MemberId = membersToAdd[i].Id;
            }

            List<Apprentice> result;

            using (var context = new AanDataContext(_dbContextOptions))
            {
                await context.Apprentices.AddRangeAsync(apprenticesToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var sut = new ApprenticeRepository(_logger.Object, context);
                result = await sut.GetApprentices(cancellationToken, apprenticesToAdd.Select(a => a.ApprenticeId).ToArray());
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task AndEmptyArguements_ThenGetApprenticesReturnsEmpty()
        {
            ApprenticeRepository sut = new ApprenticeRepository(_logger.Object, null);

            var result = await sut.GetApprentices(cancellationToken);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AndEmptyArguements_ThenGetApprenticesByIdReturnsEmpty()
        {
            ApprenticeRepository sut = new ApprenticeRepository(_logger.Object, null);

            var result = await sut.GetApprentices(cancellationToken, []);

            Assert.That(result, Is.Empty);
        }
    }
}
