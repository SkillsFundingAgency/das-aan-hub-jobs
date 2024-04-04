using AutoFixture;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories;

public class MemberRepositoryTests
{
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
    public async Task ThenGetActiveApprenticeMembers()
    {
        var _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(AanDataContext)}_ThenGetActiveApprenticeMembers")
            .Options;

        var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                    .CreateMany(3)
                                    .ToList();

        List<Member> result;

        using (var context = new AanDataContext(_dbContextOptions))
        {
            await context.AddRangeAsync(membersToAdd);
            await context.SaveChangesAsync(cancellationToken);

            var sut = new MemberRepository(context);
            result = await sut.GetActiveApprenticeMembers(cancellationToken);
        }

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task WhenThereIsAnInactiveMember_ThenOnlyGetActiveMembers()
    {
        var _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(AanDataContext)}_ThenOnlyGetActiveMembers")
            .Options;

        var membersToAdd = _fixture.Build<Member>()
                                        .Without(m => m.MemberPreferences)
                                        .Without(m => m.Audits)
                                        .Without(m => m.Notifications)
                                        .Without(m => m.MemberProfiles)
                                    .CreateMany(3)
                                    .ToList();

        membersToAdd[2].Email = membersToAdd[2].Id.ToString();

        List<Member> result;

        using (var context = new AanDataContext(_dbContextOptions))
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
