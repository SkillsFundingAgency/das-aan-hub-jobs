using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Internal;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services;

public class NotificationServiceTests
{
    [Test, MoqAutoData]
    public async Task GetsPendingNotifications(
        [Frozen] Mock<INotificationsRepository> repoMock,
        [Frozen] Mock<IOptions<ApplicationConfiguration>> configMock,
        ApplicationConfiguration applicationConfiguration,
        NotificationService sut)
    {
        configMock.SetupGet(c => c.Value).Returns(applicationConfiguration);

        await sut.ProcessNotificationBatch();

        repoMock.Verify(c => c.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize));
    }

    [Test, MoqAutoData]
    public async Task SendsMessageForEachNotification(
        [Frozen] Mock<INotificationsRepository> repoMock,
        [Frozen] Mock<IOptions<ApplicationConfiguration>> configMock,
        ApplicationConfiguration applicationConfiguration,
        List<Notification> notifications,
        NotificationService sut)
    {
        configMock.SetupGet(c => c.Value).Returns(applicationConfiguration);
        repoMock.Setup(r => r.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize)).ReturnsAsync(notifications);

        var actual = await sut.ProcessNotificationBatch();

        repoMock.Verify(c => c.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize));
        actual.Should().Be(notifications.Count);
    }
}
