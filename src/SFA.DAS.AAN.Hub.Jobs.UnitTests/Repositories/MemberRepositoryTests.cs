using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.FixtureSpecimens;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Repositories;

public class MemberRepositoryTests
{
    private DbContextOptions<AanDataContext> _dbContextOptions;
    private Mock<ILogger<MemberRepository>> _logger;
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

        _logger = new Mock<ILogger<MemberRepository>>();
    }

    [Test]
    public async Task ThenGetMembers()
    {
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

            var sut = new MemberRepository(_logger.Object, context);
            result = await sut.GetMembers(membersToAdd.Select(a => a.Id).ToArray(), cancellationToken);
        }

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void AndEmptyArguements_ThenGetMembersThrowsException()
    {
        Mock<IAanDataContext> contextMock = new Mock<IAanDataContext>();

        contextMock.Setup(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Error occurred while saving changes"));

        MemberRepository sut = new MemberRepository(_logger.Object, contextMock.Object);

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await sut.GetMembers([], cancellationToken);
        });
    }

    [Test]
    public async Task ThenUpdateMembersIsSuccessful()
    {
        var member = _fixture.Build<Member>()
                                    .Without(m => m.MemberPreferences)
                                    .Without(m => m.Audits)
                                    .Without(m => m.Notifications)
                                    .Without(m => m.MemberProfiles)
                                .Create();

        string firstName = member.FirstName;

        Member? updatedMember;

        using (var context = new AanDataContext(_dbContextOptions))
        {
            await context.Members.AddAsync(member);
            await context.SaveChangesAsync();

            member.FirstName = $"{member.FirstName}_updated";

            var sut = new MemberRepository(_logger.Object, context);
            await sut.UpdateMembers([member], cancellationToken);
            updatedMember = context.Members.FirstOrDefault(t => t.Id == member.Id);
        }

        Assert.That(updatedMember, Is.Not.Null);
        Assert.That(updatedMember.FirstName, Is.EqualTo($"{firstName}_updated"));
    }

    [Test]
    public void AndEmptyArguements_ThenUpdateMembersThrowsException()
    {
        Mock<IAanDataContext> contextMock = new Mock<IAanDataContext>();

        contextMock.Setup(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Error occurred while saving changes"));

        var mockDbSet = MockSet<Member>();

        contextMock.Setup(a => a.Members).Returns(mockDbSet.Object);

        MemberRepository sut = new MemberRepository(_logger.Object, contextMock.Object);

        Assert.ThrowsAsync<Exception>(async () =>
        {
            await sut.UpdateMembers([], cancellationToken);
        });
    }

    private Mock<DbSet<T>> MockSet<T>() where T : class
    {
        var members = new List<T>();

        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(members.AsQueryable().Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(members.AsQueryable().Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(members.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => members.AsQueryable().GetEnumerator());

        return mockDbSet;
    }
}
