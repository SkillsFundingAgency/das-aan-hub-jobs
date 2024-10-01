using AutoFixture;
using Moq;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.AAN.Hub.Jobs.UnitTests.Extensions;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.Notifications.Messages.Commands;
using NServiceBus.Features;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Common;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services
{
    public class EventSignUpNotificationServiceTests
    {
        private readonly Mock<IEventSignUpNotificationRepository> _mockEventSignUpNotificationRepository;
        private readonly Mock<IMemberRepository> _mockMemberRepository;
        private readonly Mock<IMessageSession> _mockMessageSession;
        private readonly Mock<ILogger<EventSignUpNotificationService>> _mockLogger;
        private readonly EventSignUpNotificationService _sut;
        private Mock<IOptions<ApplicationConfiguration>> _optionsMock = null!;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private CancellationToken _cancellationToken;

        public EventSignUpNotificationServiceTests()
        {
            Fixture fixture = new();

            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _cancellationToken = fixture.Create<CancellationToken>();

            _mockEventSignUpNotificationRepository = new Mock<IEventSignUpNotificationRepository>();
            _mockMemberRepository = new Mock<IMemberRepository>();
            _mockMessageSession = new Mock<IMessageSession>();
            _mockLogger = new Mock<ILogger<EventSignUpNotificationService>>();

            var notifications = new NotificationsConfiguration
            {
                Templates = new Dictionary<string, string>
                {
                    { "AANAdminEventSignup", Guid.NewGuid().ToString() }
                }
            };
            _applicationConfiguration = fixture.Build<ApplicationConfiguration>()
                .With(c => c.Notifications, notifications)
                .Create();
            _optionsMock = new Mock<IOptions<ApplicationConfiguration>>();
            _optionsMock.SetupGet(c => c.Value).Returns(_applicationConfiguration);

            _sut = new EventSignUpNotificationService(
                _mockEventSignUpNotificationRepository.Object,
                _mockMemberRepository.Object,
                _mockMessageSession.Object,
                _optionsMock.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task ProcessEventSignUpNotification_NoPendingNotifications_ReturnsZero()
        {
            // Arrange
            _mockEventSignUpNotificationRepository
                .Setup(x => x.GetEventSignUpNotification())
                .ReturnsAsync(new List<EventSignUpNotification>());

            // Act
            var result = await _sut.ProcessEventSignUpNotification(CancellationToken.None);

            // Assert
            result.Should().Be(0);
            _mockMessageSession.Verify(x => x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Never);
        }

        [Test]
        public async Task ProcessEventSignUpNotification_WithPendingNotifications_SendsEmailsAndReturnsOne()
        {
            // Arrange
            var eventSignUpNotifications = new List<EventSignUpNotification>
            {
                new EventSignUpNotification
                {
                    AdminMemberId = Guid.NewGuid(),
                    EventTitle = "Test Event",
                    FirstName = "Hikkelokke",
                    CalendarEventId = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow
                }
            };

            var adminDetails = new MemberDetails { FirstName = "Hikkelokke", Email = "hikkelokke@example.com" };
            var templateId = _applicationConfiguration.Notifications.Templates["AANAdminEventSignup"];

            _mockEventSignUpNotificationRepository
                .Setup(x => x.GetEventSignUpNotification())
                .ReturnsAsync(eventSignUpNotifications);

            _mockMessageSession
                .Setup(x => x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ProcessEventSignUpNotification(CancellationToken.None);

            // Assert
            result.Should().Be(1);

            _mockMessageSession.Verify(
                x => x.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()),
                Times.Once
            );
        }
    }

}
