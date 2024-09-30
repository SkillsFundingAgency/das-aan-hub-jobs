using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SFA.DAS.AAN.Hub.Jobs.Constant.Constants;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface INotificationService
{
    Task<int> ProcessNotificationBatch(CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    public const string LinkTokenKey = "link";

    private readonly INotificationsRepository _notificationRepository;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IAanDataContext _aanDataContext;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationsRepository notificationRepository,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        IAanDataContext aanDataContext,
        IMessageSession messageSession, ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _aanDataContext = aanDataContext;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task<int> ProcessNotificationBatch(CancellationToken cancellationToken)
    {
        var pendingNotifications = await _notificationRepository.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize);

        if (pendingNotifications.Count == 0) return 0;

        var tasks = pendingNotifications.Select(n => SendNotification(n));

        await Task.WhenAll(tasks);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingNotifications.Count;
    }

    private async Task SendNotification(Notification notification)
    {
        try
        {
            var command = CreateCommand(notification);
            await _messageSession.Send(command);
            notification.SentTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            var notificationId = notification.Id;
            // catch all exceptions to allow other notifications to go forward
            _logger.LogError(ex, "Error sending out notification with id: {notificationId}", notificationId);
        }
    }

    private SendEmailCommand CreateCommand(Notification notification)
    {
        var templateId = _applicationConfiguration.Notifications.Templates[notification.TemplateName];
        var tokens = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);
        var adminBaseUrl = _applicationConfiguration.AdminAanBaseUrl.ToString();

        if (EmailTemplateName.AdminEventAttendanceCancelTemplate == notification.TemplateName)
        {
            tokens.Add("manageeventlink", $"{adminBaseUrl}events/{tokens["calendarEventId"]}");
            tokens.Add("alleventslink", $"{adminBaseUrl}events");
            tokens.Add("unsubscribelink", $"{adminBaseUrl}notification-settings");
        }
        else
        {
            // add link token
            var link = new Uri(notification.Member.UserType == UserType.Apprentice ? _applicationConfiguration.ApprenticeAanBaseUrl : _applicationConfiguration.EmployerAanBaseUrl, $"links/{notification.Id}");
            tokens.Add(LinkTokenKey, link.ToString());
        }

        return new(templateId, notification.Member.Email, tokens);
    }
}
