using AutoFixture;
using NUnit.Framework;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.ApprenticeMembers
{
    public class MemberWithApprenticeUserType
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
        public async Task Only_members_with_apprentice_user_type_are_returned()
        {
            var membersToAdd = _fixture.Build<Member>()
                                            .Without(m => m.MemberPreferences)
                                            .Without(m => m.Audits)
                                            .Without(m => m.Notifications)
                                            .Without(m => m.MemberProfiles)
                                        .CreateMany(3)
                                        .ToList();
            List<Member> result;

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(Only_members_with_apprentice_user_type_are_returned)}_InMemoryContext"))
            {
                await context.AddRangeAsync(membersToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var sut = new MemberRepository(context);
                result = await sut.GetActiveApprenticeMembers(cancellationToken);
            }

            var apprenticeCount = membersToAdd.Where(a => a.UserType == UserType.Apprentice).Count();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(apprenticeCount));
        }

        [Test]
        public async Task Only_active_members_are_returned()
        {
            var membersToAdd = _fixture.Build<Member>()
                                            .With(m => m.UserType, UserType.Apprentice)
                                            .Without(m => m.MemberPreferences)
                                            .Without(m => m.Audits)
                                            .Without(m => m.Notifications)
                                            .Without(m => m.MemberProfiles)
                                        .CreateMany(3)
                                        .ToList();

            membersToAdd[2].Email = membersToAdd[2].Id.ToString();

            List<Member> result;

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(Only_active_members_are_returned)}_InMemoryContext"))
            {
                await context.AddRangeAsync(membersToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var sut = new MemberRepository(context);
                result = await sut.GetActiveApprenticeMembers(cancellationToken);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
        }
    }
}
