using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    Task<int> ProcessNotificationBatch(CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    public const string UserTypeApprentice = "Apprentice";
    public const string LinkTokenKey = "link";

    private readonly INotificationsRepository _notificationRepository;
    private readonly IOptions<ApplicationConfiguration> _applicationConfigurationOptions;
    private readonly IAanDataContext _aanDataContext;
    private readonly IMessageSession _messageSession;

    public NotificationService(
        INotificationsRepository notificationRepository,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        IAanDataContext aanDataContext,
        IMessageSession messageSession)
    {
        _notificationRepository = notificationRepository;
        _applicationConfigurationOptions = applicationConfigurationOptions;
        _aanDataContext = aanDataContext;
        _messageSession = messageSession;
    }

    public async Task<int> ProcessNotificationBatch(CancellationToken cancellationToken)
    {
        var applicationConfiguration = _applicationConfigurationOptions.Value;

        var pendingNotifications = await _notificationRepository.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize);

        if (!pendingNotifications.Any()) return 0;

        var commands = pendingNotifications.Select(n => CreateCommand(n, applicationConfiguration));

        var tasks = commands.Select(c => _messageSession.Send(c));

        var now = DateTime.UtcNow;
        pendingNotifications.ForEach(n => n.SentTime = now);

        await Task.WhenAll(tasks);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingNotifications.Count;
    }

    private static SendEmailCommand CreateCommand(Notification notification, ApplicationConfiguration applicationConfiguration)
    {
        var templateId = applicationConfiguration.Notifications.Templates[notification.TemplateName];
        var tokens = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);
        var link = new Uri(notification.Member.UserType == UserTypeApprentice ? applicationConfiguration.ApprenticeAanBaseUrl : applicationConfiguration.EmployerAanBaseUrl, $"links/{notification.Id}");
        tokens.Add(LinkTokenKey, link.ToString());

        return new(templateId, notification.Member.Email, tokens);
    }
}
