using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SFA.DAS.AAN.Hub.Data.Helpers;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IEventNotificationService
{
    Task<int> ProcessEventNotifications(CancellationToken cancellationToken);
}

public class EventNotificationService : IEventNotificationService
{
    private readonly IEventNotificationSettingsRepository _memberRepository;
    private readonly ILogger<EventNotificationService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IMessageSession _messageSession;
    private readonly IEventQueryService _eventQueryService;

    public EventNotificationService(
       IEventNotificationSettingsRepository memberRepository,
       IMessageSession messageSession,
       IOptions<ApplicationConfiguration> applicationConfigurationOptions,
       ILogger<EventNotificationService> logger,
       IEventQueryService eventQueryService)
    {
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
        _eventQueryService = eventQueryService;
    }

    public async Task<int> ProcessEventNotifications(CancellationToken cancellationToken)
    {
        var notificationSettings = await _memberRepository.GetEventNotificationSettingsAsync(cancellationToken);

        _logger.LogInformation("Number of members receiving event notifications: {count}.", notificationSettings.Count);

        if (notificationSettings.Count == 0) return 0;

        var tasks = notificationSettings.Select(n => SendEventNotificationEmails(n, cancellationToken));

        await Task.WhenAll(tasks);

        return notificationSettings.Count;
    }

    private async Task SendEventNotificationEmails(EventNotificationSettings notificationSettings, CancellationToken cancellationToken)
    {
        try
        {
            var eventFormats = EventFormatParser.GetEventFormats(notificationSettings);

            var eventListings = await _eventQueryService.GetEventListings(notificationSettings, eventFormats, cancellationToken);

            var totalCount = eventListings.Sum(e => e.TotalCount);

            // SENDER
            var command = CreateSendCommand(notificationSettings, eventListings, totalCount, cancellationToken);
            _logger.LogInformation("Sending email to member {memberId}.", notificationSettings.MemberDetails.Id);
            await _messageSession.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED to {memberId}!", notificationSettings.MemberDetails.Id);
        }
    }

    private SendEmailCommand CreateSendCommand(EventNotificationSettings notificationSetting, List<EventListingDTO> events, int numberOfEvents, CancellationToken cancellationToken)
    {
        var targetEmail = notificationSetting.MemberDetails.Email;
        var firstName = notificationSetting.MemberDetails.FirstName;
        var unsubscribeURL = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "accounts/" + "TODO/" + "event-notification-settings"; // TODO

        var tokens = new Dictionary<string, string>
            {
                { "first_name", firstName },
                { "event_count", numberOfEvents.ToString() }, // TODO
                { "event_listing_snippet", GetEventListingSnippet(events) }, // TODO
                { "event_formats_snippet", GetEventFormatsSnippet(notificationSetting) },
                { "locations_snippet", GetLocationsSnippet(notificationSetting) },
                { "unsubscribe_url", unsubscribeURL}
            };

        var templateId = _applicationConfiguration.Notifications.Templates["AANEmployerEventNotifications"];

        return new SendEmailCommand(templateId, targetEmail, tokens);
    }

    private string GetEventFormatsSnippet(EventNotificationSettings notificationSettings)
    {
        var sb = new StringBuilder();

        foreach (var e in notificationSettings.EventTypes)
        {
            if (e.EventType == "InPerson")
            {
                sb.AppendLine($"* in-person events");
                break;
            }

            sb.AppendLine($"* {e.EventType} events");
        }

        return sb.ToString();
    }

    private string GetLocationsSnippet(EventNotificationSettings notificationSettings)
    {
        var sb = new StringBuilder();

        foreach (var loc in notificationSettings.Locations)
        {
            var locationText = loc.Radius == 0 ? "Across England" : $"* {loc.Name}, within {loc.Radius} miles";
            sb.AppendLine(locationText);
        }

        return sb.ToString();
    }

    private string GetEventListingSnippet(List<EventListingDTO> eventListingPerLocation)
    {
        var sb = new StringBuilder();

        foreach (var e in eventListingPerLocation) 
        {
            var locationText = e.Radius == 0 ? "Across England" : $"* {e.Location}, within {e.Radius} miles";
            sb.AppendLine(locationText);

            foreach (var calendarEvent in e.CalendarEvents)
            {
                sb.AppendLine(calendarEvent.CalendarName);
                sb.AppendLine(calendarEvent.Summary);
                sb.AppendLine($"Date: {calendarEvent.Start.ToString()}");
                sb.AppendLine($"Time: {calendarEvent.Start.ToString()}");
                sb.AppendLine($"Where: {calendarEvent.Location.ToString()}");
                sb.AppendLine($"Distance: {calendarEvent.Distance.ToString()}");
                sb.AppendLine($"EventType: {calendarEvent.EventFormat.ToString()}");
                sb.AppendLine("---");
            }
        }

        return sb.ToString();
    }
}
