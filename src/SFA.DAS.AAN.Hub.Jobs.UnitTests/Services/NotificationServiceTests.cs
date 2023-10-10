using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Internal;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Models;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.Extensions;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.Models;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services;

public class NotificationServiceTests
{
    private Mock<INotificationsRepository> _repositoryMock = null!;
    private Mock<IOptions<ApplicationConfiguration>> _optionsMock = null!;
    private Mock<IMessagingService> _messagingServiceMock = null!;
    private ApplicationConfiguration _applicationConfiguration = null!;
    private NotificationService _sut = null!;
    private List<Notification> _notifications = new();

    [SetUp]
    public async Task Init()
    {
        Fixture fixture = new();

        _applicationConfiguration = fixture.Create<ApplicationConfiguration>();
        _optionsMock = new Mock<IOptions<ApplicationConfiguration>>();
        _optionsMock.SetupGet(c => c.Value).Returns(_applicationConfiguration);

        _notifications = fixture
            .Build<Notification>()
            .WithValues(n => n.TemplateName, _applicationConfiguration.Notifications.Templates.Keys.ToArray())
            .With(n => n.Tokens, JsonSerializer.Serialize(fixture.Create<TestTokens>()))
            .CreateMany(_applicationConfiguration.Notifications.Templates.Count)
            .ToList();
        _repositoryMock = new Mock<INotificationsRepository>();
        _repositoryMock.Setup(r => r.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize)).ReturnsAsync(_notifications);

        _messagingServiceMock = new Mock<IMessagingService>();

        _sut = new(_repositoryMock.Object, _optionsMock.Object, _messagingServiceMock.Object);

        await _sut.ProcessNotificationBatch();
    }

    [Test]
    public void ThenGetsPendingNotifications() =>
        _repositoryMock.Verify(c => c.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize));

    [Test]
    public void ThenSendsMessageForEachNotification() =>
        _messagingServiceMock.Verify(m => m.SendMessage(It.IsAny<SendEmailCommand>()), Times.Exactly(_notifications.Count));
}
