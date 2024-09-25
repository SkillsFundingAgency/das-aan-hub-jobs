using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SFA.DAS.AAN.Hub.Data.Dto;
using Microsoft.Extensions.Options;
using System.Text;
using System.Globalization;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IEventSignUpNotificationService
{
    Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken);
}

public class EventSignUpNotificationService : IEventSignUpNotificationService
{
    private readonly IEventSignUpNotificationRepository _eventSignUpNotificationRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<EventSignUpNotificationService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IMessageSession _messageSession;

    public EventSignUpNotificationService(
        IEventSignUpNotificationRepository eventSignUpNotificationRepository,
        IMemberRepository memberRepository,
        IMessageSession messageSession,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        ILogger<EventSignUpNotificationService> logger)
    {
        _eventSignUpNotificationRepository = eventSignUpNotificationRepository;
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
    }

    public async Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken)
    {
        // get all events
        var pendingEventSignUpNotifications = await _eventSignUpNotificationRepository.GetEventSignUpNotification();

        _logger.LogInformation("Number of members signed up to an event in the last 24 hours: {count}.", pendingEventSignUpNotifications.Count);

        if (pendingEventSignUpNotifications.Count == 0) return 0;

        // group events per admin id
        var notificationPerAdmin = pendingEventSignUpNotifications.GroupBy(n => n.AdminMemberId);

        // Create a list of tasks to send notifications
        var tasks = notificationPerAdmin.Select(group => SendAdminEventSignUpEmails(group.Key, group, cancellationToken));

        await Task.WhenAll(tasks);

        return pendingEventSignUpNotifications.Count;
    }

    private async Task SendAdminEventSignUpEmails(Guid memberId, IEnumerable<EventSignUpNotification> events, CancellationToken cancellationToken)
    {
        try
        {
            var command = CreateSendCommand(events, cancellationToken);
            _logger.LogInformation("Sending email to member {memberId}.", memberId);
            await _messageSession.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED!");
        }
    }

    private SendEmailCommand CreateSendCommand(IEnumerable<EventSignUpNotification> events, CancellationToken cancellationToken)
    {
        var adminEmail = events.First().AdminEmail;
        var adminFirstName = events.First().FirstName;
        var searchNetworkEventsURL = _applicationConfiguration.AdminAanBaseUrl.ToString() + "events";
        var notificationSettingsURL = _applicationConfiguration.AdminAanBaseUrl.ToString() + "notification-settings";

        var tokens = new Dictionary<string, string>
            {
                { "contact", adminFirstName },
                { "number_of_events", events.Count().ToString() },
                { "admin-event-listing-snippet", GetEventListingSnippet(events) },
                { "searchNetworkEventsURL", searchNetworkEventsURL },
                { "notificationSettingsURL", notificationSettingsURL}
            };

        var templateId = _applicationConfiguration.Notifications.Templates["AANAdminEventSignup"];

        return new SendEmailCommand(templateId, adminEmail, tokens);
    }


    private string GetEventListingSnippet(IEnumerable<EventSignUpNotification> notifications)
    {
        var sb = new StringBuilder();

        foreach (var n in notifications)
        {
            var manageEventUrl = _applicationConfiguration.AdminAanBaseUrl.ToString() + "events/" + n.CalendarEventId.ToString();
            var eventDates = GetCalendarDateFormat(n.StartDate, n.EndDate);

            sb.AppendLine($"# {n.EventTitle}");
            sb.AppendLine();
            sb.AppendLine($"{n.EventFormat} event");
            sb.AppendLine($"{n.CalendarName}");
            sb.AppendLine($"{eventDates}");
            sb.AppendLine();
            sb.AppendLine($"^ {n.NewAmbassadorsCount} new ambassadors signed up ({n.TotalAmbassadorsCount} total signed up)");
            sb.AppendLine();
            sb.AppendLine($"[Manage event]({manageEventUrl})");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetCalendarDateFormat(DateTime startTime, DateTime endTime) 
    {
        var formattedStartDate = startTime.ToString("dd MMMM yyyy, hh:mmtt", CultureInfo.InvariantCulture);
        formattedStartDate = formattedStartDate.Replace(" am", "am").Replace(" pm", "pm");

        var formattedEndDateHour = endTime.ToString("htt", CultureInfo.InvariantCulture);
        formattedEndDateHour = formattedEndDateHour.Replace(" am", "am").Replace(" pm", "pm");
        return formattedStartDate + " to " + formattedEndDateHour;
    }
}
