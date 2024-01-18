using AutoFixture;
using Moq;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services;
public class MemberDataCleanupServiceTests
{
    private Mock<IMemberDataCleanupRepository> _repositoryMock = null!;
    private Mock<IAanDataContext> _contextMock = null!;
    private CancellationToken _cancellationToken;
    private List<Member> _membersWithdrawnOrDeleted = null!;
    private List<Member> _membersRemoved = null!;
    private MemberDataCleanupService _sut = null!;

    [SetUp]
    public async Task Init()
    {
        _contextMock = new();

        Fixture fixture = new();
        _cancellationToken = fixture.Create<CancellationToken>();

        _membersWithdrawnOrDeleted = new List<Member>
        {
            new() { Id = new Guid(), UserType = UserType.Apprentice, Email = "email2", Status = "withdrawn" },
            new() { Id = new Guid(), UserType = UserType.Employer, Email = "email3", Status = "deleted"}
        };

        _membersRemoved = new List<Member>
        {
            new() { Id = new Guid(), UserType = UserType.Employer, Email = "email1", Status = "removed" }
        };
        _repositoryMock = new Mock<IMemberDataCleanupRepository>();
        _repositoryMock.Setup(x => x.GetWithdrawnOrDeletedMembers()).ReturnsAsync(_membersWithdrawnOrDeleted);
        _repositoryMock.Setup(x => x.GetRemovedMembers()).ReturnsAsync(_membersRemoved);

        _sut = new MemberDataCleanupService(_contextMock.Object, _repositoryMock.Object);

        await _sut.ProcessMemberDataCleanup(_cancellationToken);
    }

    [Test]
    public void ThenGetsWithdrawnOrDeletedMembersToCleanup() =>
        _repositoryMock.Verify(x => x.GetWithdrawnOrDeletedMembers());

    [Test]
    public void ThenGetRemovedMembersToCleanup() =>
        _repositoryMock.Verify(x => x.GetRemovedMembers());

    [Test]
    public void ThenUpdatesEachMember() =>
        _repositoryMock.Verify(x => x.UpdateMemberDetails(It.IsAny<Member>(), _cancellationToken), Times.Exactly(3));

    [Test]
    public void ThenDeletesEachMemberProfile() =>
        _repositoryMock.Verify(x => x.DeleteMemberProfile(It.IsAny<List<MemberProfile>>(), _cancellationToken),
            Times.Exactly(3));

    [Test]
    public void ThenDeletesEachMemberPreference() =>
        _repositoryMock.Verify(x => x.DeleteMemberPreference(It.IsAny<List<MemberPreference>>(), _cancellationToken),
            Times.Exactly(3));

    [Test]
    public void ThenDeletesEachMemberNotifications() =>
        _repositoryMock.Verify(x => x.DeleteMemberNotifications(It.IsAny<List<Notification>>(), _cancellationToken),
            Times.Exactly(3));

    [Test]
    public void ThenDeletesEachMemberAudit() =>
        _repositoryMock.Verify(x => x.DeleteMemberAudit(It.IsAny<List<Audit>>(), _cancellationToken),
            Times.Exactly(3));

    [Test]
    public void ThenDeletesEachApprenticeMember() =>
        _repositoryMock.Verify(x => x.DeleteMemberApprentice(It.IsAny<Member>(), _cancellationToken), Times.Once());

    [Test]
    public void ThenDeletesEachEmployerMember() =>
        _repositoryMock.Verify(x => x.DeleteMemberEmployer(It.IsAny<Member>(), _cancellationToken), Times.Exactly(2));

    [Test]
    public void ThenUpdatesEachMemberFutureAttendance() =>
        _repositoryMock.Verify(x => x.UpdateMemberFutureAttendance(It.IsAny<Member>(), _cancellationToken),
            Times.Exactly(3));
}
