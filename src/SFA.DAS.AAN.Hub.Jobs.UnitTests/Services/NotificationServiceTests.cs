using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using NServiceBus;
using NUnit.Framework.Internal;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.Extensions;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services;

public class NotificationServiceTests
{
    private Mock<INotificationsRepository> _repositoryMock = null!;
    private Mock<IOptions<ApplicationConfiguration>> _optionsMock = null!;
    private Mock<IMessageSession> _messagingSessionMock = null!;
    private Mock<IAanDataContext> _contextMock = null!;
    private CancellationToken _cancellationToken;
    private ApplicationConfiguration _applicationConfiguration = null!;
    private List<Notification> _notifications = new();
    private NotificationService _sut = null!;

    [SetUp]
    public async Task Init()
    {
        _contextMock = new();

        Fixture fixture = new();
        _cancellationToken = fixture.Create<CancellationToken>();

        _applicationConfiguration = fixture.Create<ApplicationConfiguration>();
        _optionsMock = new Mock<IOptions<ApplicationConfiguration>>();
        _optionsMock.SetupGet(c => c.Value).Returns(_applicationConfiguration);

        _notifications = fixture
            .Build<Notification>()
            .WithValues(n => n.TemplateName, _applicationConfiguration.Notifications.Templates.Keys.ToArray())
            .With(n => n.Tokens, System.Text.Json.JsonSerializer.Serialize(fixture.Create<TestTokens>()))
            .CreateMany(_applicationConfiguration.Notifications.Templates.Count)
            .ToList();
        _repositoryMock = new Mock<INotificationsRepository>();
        _repositoryMock.Setup(r => r.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize)).ReturnsAsync(_notifications);

        _messagingSessionMock = new Mock<IMessageSession>();

        _sut = new(_repositoryMock.Object, _optionsMock.Object, _contextMock.Object, _messagingSessionMock.Object);

        await _sut.ProcessNotificationBatch(_cancellationToken);
    }

    [Test]
    public void ThenGetsPendingNotifications() =>
        _repositoryMock.Verify(c => c.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize));

    [Test]
    public void ThenSendsMessageForEachNotification() =>
        _messagingSessionMock.Verify(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Exactly(_notifications.Count));

    [Test]
    public void ThenUpdatesAllTheNotifications() => _contextMock.Verify(c => c.SaveChangesAsync(_cancellationToken));

    [Test]
    public void ThenConvertsNotificationToSendEmailCommand()
    {
        var notification = _notifications.First();
        var templateId = _applicationConfiguration.Notifications.Templates[notification.TemplateName];
        _messagingSessionMock.Verify(m => m.Send(It.Is<SendEmailCommand>(c => c.TemplateId == templateId && c.RecipientsAddress == notification.Member.Email), It.IsAny<SendOptions>()));
    }

    [Test]
    public void ThenAddsLinksBaseOnMemberUserType()
    {
        var notification = _notifications.First();

        var templateId = _applicationConfiguration.Notifications.Templates[notification.TemplateName];

        var uri = notification.Member.UserType == NotificationService.UserTypeApprentice ? _applicationConfiguration.ApprenticeAanRouteUrl : _applicationConfiguration.EmployerAanRouteUrl;

        _messagingSessionMock.Verify(m => m.Send(It.Is<SendEmailCommand>(c => c.TemplateId == templateId && c.Tokens[NotificationService.LinkTokenKey].Contains(uri.ToString())), It.IsAny<SendOptions>()));
    }
}

public record TestTokens(string Name, string Id);
