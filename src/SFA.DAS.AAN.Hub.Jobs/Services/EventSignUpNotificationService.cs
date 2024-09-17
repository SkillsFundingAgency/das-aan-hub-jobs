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
using System.Text;

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
        // get all events
        var pendingEventSignUpNotifications = await _eventSignUpNotificationRepository.GetEventSignUpNotification();

        if (pendingEventSignUpNotifications.Count == 0) return 0;

        // group events per admin id
        var notificationPerAdmin = pendingEventSignUpNotifications.GroupBy(n => n.AdminMemberId);

        // send notification per admin
        var tasks = notificationPerAdmin.Select(async n =>
        {
            var email = "TODO: get email";

            var tokens = new Dictionary<string, string>
            {
                { "admin-event-listing-snippet", GetEventListingSnippet(n) }
            };
            var templateId = _applicationConfiguration.Notifications.Templates["TODO_templateName"];

            var command = new SendEmailCommand(templateId, email, tokens);

            await _messageSession.Send(command);
        });

        await Task.WhenAll(tasks);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingEventSignUpNotifications.Count;
    }

    private string GetEventListingSnippet(IEnumerable<EventSignUpNotification> notifications)
    {
        var sb = new StringBuilder();

        foreach (var n in notifications)
        {
            sb.AppendLine($"# {n.EventTitle}");
            sb.AppendLine();
            sb.AppendLine($"{n.EventFormat}");
            sb.AppendLine($"{n.CalendarName}");
            sb.AppendLine($"{n.StartDate}");
            sb.AppendLine();
            sb.AppendLine($"^ {n.NewAmbassadorsCount} new ambassadors signed up ({n.TotalAmbassadorsCount} total signed up)");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
