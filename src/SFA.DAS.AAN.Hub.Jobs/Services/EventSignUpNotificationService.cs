using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data;
using Microsoft.Extensions.Options;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IEventSignUpNotificationService
{
    Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken);
}

public class EventSignUpNotificationService : IEventSignUpNotificationService
{
    public const string LinkTokenKey = "link";

    private readonly IEventSignUpNotificationRepository _eventSignUpNotificationRepository;
    private readonly ILogger<NotificationService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IMessageSession _messageSession;
    private readonly IAanDataContext _aanDataContext;

    public EventSignUpNotificationService(
        IEventSignUpNotificationRepository eventSignUpNotificationRepository,
        IMessageSession messageSession,
        IAanDataContext aanDataContext,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        ILogger<NotificationService> logger)
    {
        _eventSignUpNotificationRepository = eventSignUpNotificationRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _aanDataContext = aanDataContext;
        _logger = logger;
    }

    public async Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken)
    {
        var pendingEventSignUpNotifications = await _eventSignUpNotificationRepository.GetEventSignUpNotification();

        if (pendingEventSignUpNotifications.Count == 0) return 0;

        var tasks = pendingEventSignUpNotifications.Select(n => SendNotification(n));

        await Task.WhenAll(tasks);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingEventSignUpNotifications.Count;
    }

    private async Task SendNotification(EventSignUpNotification notification)
    {
        try
        {
            var command = CreateCommand(notification);
            await _messageSession.Send(command);
            // notification.SentTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // var notificationId = notification.Id;
            // catch all exceptions to allow other notifications to go forward
            _logger.LogError(ex, "Error sending out notification for event: {eventTitle}", notification.EventTitle);
        }
    }

    private SendEmailCommand CreateCommand(EventSignUpNotification notification)
    {
        var email ="TODO: get email";
        var tokens = new Dictionary<string, string>
        {
            { "EventTitle", notification.EventTitle },
            { "EventFormat", notification.EventFormat },
            { "StartDate", notification.StartDate.ToString("dd/MM/yyyy") },
            { "EndDate", notification.EndDate.ToString("dd/MM/yyyy") },
            { "FirstName", notification.FirstName },
            { "LastName", notification.LastName },
            { "NewAmbassadorsCount", notification.NewAmbassadorsCount.ToString() },
            { "TotalAmbassadorsCount", notification.TotalAmbassadorsCount.ToString() }
        };
        var templateId = _applicationConfiguration.Notifications.Templates["TODO_templateName"];

        return new(templateId, email, tokens);
    }
}
