using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface INotificationService
{
    Task<int> ProcessNotificationBatch(ILogger logger, CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    public const string UserTypeApprentice = "Apprentice";
    public const string LinkTokenKey = "link";

    private readonly INotificationsRepository _notificationRepository;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IAanDataContext _aanDataContext;
    private readonly IMessageSession _messageSession;

    public NotificationService(
        INotificationsRepository notificationRepository,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        IAanDataContext aanDataContext,
        IMessageSession messageSession)
    {
        _notificationRepository = notificationRepository;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _aanDataContext = aanDataContext;
        _messageSession = messageSession;
    }

    public async Task<int> ProcessNotificationBatch(ILogger logger, CancellationToken cancellationToken)
    {
        var pendingNotifications = await _notificationRepository.GetPendingNotifications(_applicationConfiguration.Notifications.BatchSize);

        if (!pendingNotifications.Any()) return 0;

        var tasks = pendingNotifications.Select(n => SendNotification(n, logger, cancellationToken));

        await Task.WhenAll(tasks);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingNotifications.Count;
    }

    private async Task SendNotification(Notification notification, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var command = CreateCommand(notification);
            await _messageSession.Send(command);
            notification.SentTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // catch all exceptions to allow other notifications to go forward
            logger.LogError(ex, $"Error sending out notification with id: {notification.Id}");
        }
    }

    private SendEmailCommand CreateCommand(Notification notification)
    {
        var templateId = _applicationConfiguration.Notifications.Templates[notification.TemplateName];
        var tokens = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);

        // add link token
        var link = new Uri(notification.Member.UserType == UserTypeApprentice ? _applicationConfiguration.ApprenticeAanBaseUrl : _applicationConfiguration.EmployerAanBaseUrl, $"links/{notification.Id}");
        tokens.Add(LinkTokenKey, link.ToString());

        return new(templateId, notification.Member.Email, tokens);
    }
}
