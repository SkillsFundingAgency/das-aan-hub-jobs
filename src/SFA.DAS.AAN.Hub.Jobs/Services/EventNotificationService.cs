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
using SFA.DAS.AAN.Hub.Data.Entities;

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
    public readonly IEmployerAccountsService _employerAccountsService;

    public EventNotificationService(
       IEventNotificationSettingsRepository memberRepository,
       IMessageSession messageSession,
       IOptions<ApplicationConfiguration> applicationConfigurationOptions,
       ILogger<EventNotificationService> logger,
       IEventQueryService eventQueryService,
       IEmployerAccountsService employerAccountsService)
    {
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
        _eventQueryService = eventQueryService;
        _employerAccountsService = employerAccountsService;
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

            var employerAccounts = await _employerAccountsService.GetEmployerUserAccounts(notificationSettings.MemberDetails.Id.ToString(), notificationSettings.MemberDetails.Email);

            _logger.LogInformation("{count} employer accounts found for member {memberId}.", employerAccounts.UserAccounts.Count(), notificationSettings.MemberDetails.Id);

            var command = CreateSendCommand(notificationSettings, eventListings, employerAccounts.UserAccounts.First().AccountId, cancellationToken);

            _logger.LogInformation("Sending email to member {memberId}.", notificationSettings.MemberDetails.Id);

            await _messageSession.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED to {memberId}!", notificationSettings.MemberDetails.Id);
        }
    }

    private SendEmailCommand CreateSendCommand(EventNotificationSettings notificationSetting, List<EventListingDTO> events, string employerAccountId, CancellationToken cancellationToken)
    {
        var targetEmail = notificationSetting.MemberDetails.Email;
        var firstName = notificationSetting.MemberDetails.FirstName;
        _logger.LogInformation("Employer Account used: {employerAccountId}.", employerAccountId);
        var unsubscribeURL = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "accounts/" + employerAccountId.ToString() + "/event-notification-settings"; // TODO
        var eventCount = events.Sum(e => e.TotalCount);

        var tokens = new Dictionary<string, string>
            {
                { "first_name", firstName },
                { "event_count", eventCount.ToString() },
                { "event_listing_snippet", GetEventListingSnippet(events, employerAccountId) }, // TODO
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

            sb.AppendLine($"* {e.EventType.ToLower()} events");
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

    private string GetEventListingSnippet(List<EventListingDTO> eventListings, string employerAccountId)
    {
        var sb = new StringBuilder();

        var inPersonAndHybridEvents = eventListings
            .Where(e => e.CalendarEvents.Any(ev => ev.EventFormat == EventFormat.InPerson || ev.EventFormat == EventFormat.Hybrid))
            .ToList();

        var onlineEvents = eventListings
            .Where(e => e.CalendarEvents.All(ev => ev.EventFormat == EventFormat.Online))
            .ToList();

        var inPersonAndHybridTotalCount = inPersonAndHybridEvents
            .Sum(e => e.CalendarEvents.Count(ev => ev.EventFormat == EventFormat.InPerson || ev.EventFormat == EventFormat.Hybrid));

        var onlineTotalCount = onlineEvents
            .Sum(e => e.CalendarEvents.Count(ev => ev.EventFormat == EventFormat.Online));

        // Process In-Person and Hybrid Events
        if (inPersonAndHybridEvents.Any())
        {
            sb.AppendLine($"#In-person and hybrid ({inPersonAndHybridTotalCount} events)");
            sb.AppendLine();

            foreach (var locationEvents in inPersonAndHybridEvents)
            {
                AppendLocationEvents(sb, locationEvents, employerAccountId, EventFormat.InPerson, EventFormat.Hybrid);
            }
        }

        // Process Online Events
        if (onlineEvents.Any())
        {
            sb.AppendLine($"#Online events ({onlineTotalCount} events)");
            sb.AppendLine();

            foreach (var locationEvents in onlineEvents)
            {
                AppendLocationEvents(sb, locationEvents, employerAccountId, EventFormat.Online);
            }
        }

        return sb.ToString();
    }


    private void AppendLocationEvents(StringBuilder sb, EventListingDTO locationEvents, string employerAccountId, params EventFormat[] formatsToInclude)
    {
        var filteredEvents = locationEvents.CalendarEvents
            .Where(ev => formatsToInclude.Contains(ev.EventFormat))
            .ToList();

        if (!filteredEvents.Any())
            return;

        var locationHeaderText = locationEvents.Radius == 0
            ? $"##Across England ({filteredEvents.Count} events)"
            : $"##{locationEvents.Location}, within {locationEvents.Radius} miles ({filteredEvents.Count} events)";
        sb.AppendLine(locationHeaderText);
        sb.AppendLine();

        var locationUrlText = locationEvents.Radius == 0
            ? $"across England"
            : $"within {locationEvents.Radius} miles  of {locationEvents.Location}";

        var maxEventsPerLocation = 3;
        var eventsDisplayed = 0;

        foreach (var calendarEvent in filteredEvents)
        {
            var calendarEventUrl = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "accounts/" + employerAccountId.ToString() + "/network-events/" + calendarEvent.CalendarEventId.ToString();
            var allEventsUrl = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "accounts/" + employerAccountId.ToString() + "/network-events";

            sb.AppendLine($"##[{calendarEvent.Title}]({calendarEventUrl})");
            sb.AppendLine();
            sb.AppendLine(calendarEvent.Summary);
            sb.AppendLine();
            sb.AppendLine($"Date: {calendarEvent.Start.ToString("dd MMMM yyyy")}");
            sb.AppendLine($"Time: {calendarEvent.Start.ToString("htt").ToUpper()} to {calendarEvent.End.ToString("htt").ToUpper()}");
            sb.AppendLine($"Where: {calendarEvent.Location}");
            if (calendarEvent.EventFormat != EventFormat.Online)
            {
                sb.AppendLine($"Distance: {calendarEvent.Distance} miles");
            }
            sb.AppendLine($"Event type: {calendarEvent.CalendarName.ToString()}");
            sb.AppendLine();

            eventsDisplayed++;

            if (eventsDisplayed >= maxEventsPerLocation)
            {
                sb.AppendLine($"^ We're only showing you the next {maxEventsPerLocation} events for {locationEvents.Location}. [See all {filteredEvents.Count} upcoming events {locationUrlText}]({calendarEventUrl}).");
                sb.AppendLine("---");
                break;
            }
            else
            {
                sb.AppendLine("---");
            }
        }

        sb.AppendLine();
    }
}