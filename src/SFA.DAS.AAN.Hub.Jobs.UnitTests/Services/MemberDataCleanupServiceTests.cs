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

        var auditsToRemove = new List<Audit>
            { new() { ActionedBy = new Guid(), Id = 1, Member = new Member(), Resource = "Apprentice" } };

        var auditsToKeep = new List<Audit>
            { new() { ActionedBy = new Guid(), Id = 1, Member = new Member(), Resource = "Member" } };

        _membersWithdrawnOrDeleted = new List<Member>
        {
            new() { Id = new Guid("311a4faf-d0d9-4f76-8e5b-2ebe6f9c357a"), UserType = UserType.Apprentice, Email = "email2", Status = MemberStatus.Withdrawn, Audits = auditsToRemove},
            new() { Id = new Guid("b82f8d01-b443-4631-94b3-72747e8292e3"), UserType = UserType.Employer, Email = "email3", Status = MemberStatus.Deleted, Audits = auditsToKeep}
        };

        _membersRemoved = new List<Member>
        {
            new() { Id = new Guid("afdbda6a-b019-48bf-ad4f-a36925e20dd8"), UserType = UserType.Employer, Email = "email1", Status = MemberStatus.Removed, Audits = auditsToRemove}
        };

        var calendarEvents = fixture.CreateMany<CalendarEvent>(3).ToList();
        var attendances = fixture.CreateMany<Attendance>(3).ToList();
        for (int i = 0; i < 3; i++)
        {
            attendances[i].CalendarEventId = calendarEvents[i].Id;
            attendances[i].MemberId = _membersWithdrawnOrDeleted[0].Id;
        }
        calendarEvents[0].StartDate = DateTime.Now.AddDays(1);
        calendarEvents[1].StartDate = DateTime.Now.AddDays(1);
        calendarEvents[2].StartDate = DateTime.Today.AddDays(-1);

        _repositoryMock = new Mock<IMemberDataCleanupRepository>();
        _repositoryMock.Setup(x => x.GetWithdrawnOrDeletedMembers()).ReturnsAsync(_membersWithdrawnOrDeleted);
        _repositoryMock.Setup(x => x.GetRemovedMembers()).ReturnsAsync(_membersRemoved);
        _repositoryMock.Setup(x => x.GetMemberAttendances(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(new List<Attendance>());
        _repositoryMock.Setup(x => x.GetMemberAttendances(_membersWithdrawnOrDeleted[0].Id, _cancellationToken))
            .ReturnsAsync(attendances);
        _repositoryMock.Setup(x => x.GetFutureCalendarEvents(It.IsAny<List<Guid>>(), _cancellationToken))
            .ReturnsAsync(calendarEvents);


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
            Times.Exactly(2));

    [Test]
    public void ThenDeletesEachApprenticeMember() =>
        _repositoryMock.Verify(x => x.DeleteMemberApprentice(It.IsAny<Apprentice>(), _cancellationToken),
            Times.Once());

    [Test]
    public void ThenDeletesEachEmployerMember() =>
        _repositoryMock.Verify(x => x.DeleteMemberEmployer(It.IsAny<Employer>(), _cancellationToken),
            Times.Exactly(2));

    [Test]
    public void ThenUpdatesEachMemberFutureAttendance() =>
        _repositoryMock.Verify(x => x.UpdateMemberFutureAttendance(It.IsAny<Attendance>(), _cancellationToken),
            Times.Exactly(2));
}
