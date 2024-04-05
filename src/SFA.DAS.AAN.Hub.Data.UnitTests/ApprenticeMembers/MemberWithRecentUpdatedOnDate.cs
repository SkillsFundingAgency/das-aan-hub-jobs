using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.ApprenticeMembers
{
    internal class MemberWithRecentUpdatedOnDate
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
        public async Task Update_details_when_sychronising_apprentices()
        {
            var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                    .CreateMany(1)
                                    .ToList();

            Member updatedMember = null!;

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(Update_details_when_sychronising_apprentices)}_InMemoryContext"))
            {
                await context.Members.AddRangeAsync(membersToAdd);
                await context.SaveChangesAsync(cancellationToken);

                var memberToUpdate = await context.Members.FirstAsync(a => a.Id == membersToAdd[0].Id);

                var sut = new SynchroniseApprenticeDetailsRepository(context);
                sut.UpdateMemberDetails(memberToUpdate, "FirstName", "LastName", "email@email.com");

                await context.SaveChangesAsync(cancellationToken);

                updatedMember = await context.Members.FirstAsync(a => a.Id == membersToAdd[0].Id);
            }

            Assert.That(updatedMember, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedMember.FirstName, Is.EqualTo("FirstName"));
                Assert.That(updatedMember.LastName, Is.EqualTo("LastName"));
                Assert.That(updatedMember.Email, Is.EqualTo("email@email.com"));
            });
        }
    }
}
