﻿using Microsoft.Extensions.Logging;
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
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IApprenticeEventNotificationService
{
    Task<int> ProcessEventNotifications(CancellationToken cancellationToken);
}

public class ApprenticeEventNotificationService : IApprenticeEventNotificationService
{
    private const int MaxEventsPerLocation = 3;

    private readonly IEventNotificationSettingsRepository _memberRepository;
    private readonly ILogger<EventNotificationService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeEventQueryService _apprenticeEventQueryService;

    public ApprenticeEventNotificationService(
       IEventNotificationSettingsRepository memberRepository,
       IMessageSession messageSession,
       IOptions<ApplicationConfiguration> applicationConfigurationOptions,
       ILogger<EventNotificationService> logger,
       IApprenticeEventQueryService apprenticeEventQueryService)
    {
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
        _apprenticeEventQueryService = apprenticeEventQueryService;
    }

    public async Task<int> ProcessEventNotifications(CancellationToken cancellationToken)
    {
        var notificationSettings = await _memberRepository.GetEventNotificationSettingsAsync(cancellationToken, UserType.Apprentice);

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
            if (notificationSettings.EventTypes.Any(x => Enum.TryParse(x.EventType, out EventFormat format) && format == EventFormat.All && x.ReceiveNotifications))
            {
                notificationSettings.EventTypes = Enum.GetValues<EventFormat>()
                    .Where(format => format != EventFormat.All)
                    .Select(format => new EventNotificationSettings.NotificationEventType
                    {
                        EventType = format.ToString(),
                        ReceiveNotifications = true
                    }).ToList();
            }
            else
            {
                notificationSettings.EventTypes.RemoveAll(x => Enum.TryParse(x.EventType, out EventFormat format) && format == EventFormat.All);
            }

            List<EventFormat> eventFormats = notificationSettings.EventTypes
                .Where(x => x.ReceiveNotifications)
                .Select(x => Enum.TryParse(x.EventType, true, out EventFormat format) ? format : (EventFormat?)null)
                .Where(format => format.HasValue)
                .Cast<EventFormat>()
                .ToList();

            var eventListingTask = _apprenticeEventQueryService.GetEventListings(notificationSettings, eventFormats, cancellationToken);

            await Task.WhenAll(eventListingTask);

            var eventListings = eventListingTask.Result;

            var eventCount = eventListings.Sum(e => e.TotalCount);

            if (eventCount > 0)
            {
                var command = CreateSendCommand(notificationSettings, eventListings, eventCount, cancellationToken);

                _logger.LogInformation("Sending email to member {memberId}.", notificationSettings.MemberDetails.Id);

                await _messageSession.Send(command);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED to {memberId}!", notificationSettings.MemberDetails.Id);
        }
    }

    private SendEmailCommand CreateSendCommand(EventNotificationSettings notificationSetting, List<EventListingDTO> events, int eventCount, CancellationToken cancellationToken)
    {
        var targetEmail = notificationSetting.MemberDetails.Email;
        var firstName = notificationSetting.MemberDetails.FirstName;
        _logger.LogInformation("Email used: {email}.", targetEmail);
        var unsubscribeURL = _applicationConfiguration.ApprenticeAanBaseUrl + "/event-notification-settings";
        var subject = eventCount == 1 ? "1 upcoming AAN event" : $"{eventCount.ToString()} upcoming AAN events";

        var tokens = new Dictionary<string, string>
            {
                { "first_name", firstName },
                { "subject", subject },
                { "event_listing_snippet", GetEventListingSnippet(events,notificationSetting) },
                { "event_formats_snippet", GetEventFormatsSnippet(notificationSetting) },
                { "locations_snippet", GetLocationsSnippet(notificationSetting) },
                { "unsubscribe_url", unsubscribeURL}
            };

        var templateId = _applicationConfiguration.Notifications.Templates["AANApprenticeEventNotifications"];

        return new SendEmailCommand(templateId, targetEmail, tokens);
    }

    private string GetEventFormatsSnippet(EventNotificationSettings notificationSettings)
    {
        var sb = new StringBuilder();

        foreach (var e in notificationSettings.EventTypes.Where(x => x.ReceiveNotifications))
        {
            if (e.EventType == "InPerson")
            {
                sb.AppendLine($"* in-person events");
            }
            else
            {
                sb.AppendLine($"* {e.EventType.ToLower()} events");
            }
        }

        return sb.ToString();
    }

    private string GetLocationsSnippet(EventNotificationSettings notificationSettings)
    {
        var sb = new StringBuilder();

        var inPersonEvents = notificationSettings.EventTypes
            .Where(x => x.EventType.Equals("InPerson", StringComparison.OrdinalIgnoreCase) && x.ReceiveNotifications);
        var hybridEvents = notificationSettings.EventTypes
            .Where(x => x.EventType.Equals("Hybrid", StringComparison.OrdinalIgnoreCase) && x.ReceiveNotifications);

        if (inPersonEvents.Any() && hybridEvents.Any())
        {
            sb.AppendLine("We'll email you about in-person and hybrid events in the following locations:");
            sb.AppendLine();
        }
        else if (inPersonEvents.Any())
        {
            sb.AppendLine("We'll email you about in-person events in the following locations:");
            sb.AppendLine();
        }
        else if (hybridEvents.Any())
        {
            sb.AppendLine("We'll email you about hybrid events in the following locations:");
            sb.AppendLine();
        }

        foreach (var loc in notificationSettings.Locations)
        {
            var locationText = loc.Radius == 0 ? $"* {loc.Name}, Across England" : $"* {loc.Name}, within {loc.Radius} miles";
            sb.AppendLine(locationText);
        }

        return sb.ToString();
    }

    private string GetEventListingSnippet(List<EventListingDTO> eventListings, EventNotificationSettings notificationSettings)
    {
        var sb = new StringBuilder();

        var inPersonAndHybridEvents = eventListings
            .Where(e => e.CalendarEvents.Any(ev => ev.EventFormat == EventFormat.InPerson || ev.EventFormat == EventFormat.Hybrid))
            .GroupBy(e => e.Location)
            .Select(g => new EventListingDTO
            {
                Location = g.First().Location,
                Radius = g.First().Radius,
                TotalCount = g.Sum(e => e.TotalCount),
                CalendarEvents = g.SelectMany(e => e.CalendarEvents).ToList()
            })
            .ToList();

        var onlineEvents = eventListings
            .Where(e => e.CalendarEvents.Any(ev => ev.EventFormat == EventFormat.Online))
            .ToList();

        var inPersonAndHybridTotalCount = inPersonAndHybridEvents.Sum(e => e.TotalCount);

        var onlineTotalCount = eventListings
            .Where(e => e.CalendarEvents.Any(ev => ev.EventFormat == EventFormat.Online))
            .Sum(e => e.TotalCount);

        var onlineEventListing = new EventListingDTO
        {
            CalendarEvents = eventListings
            .SelectMany(e => e.CalendarEvents)
            .Where(ev => ev.EventFormat == EventFormat.Online)
            .OrderBy(ev => ev.Start)
            .Take(3)
            .ToList()
        };

        if (inPersonAndHybridEvents.Any())
        {
            var inPersonEvents = notificationSettings.EventTypes
                .Where(x => x.EventType.Equals("InPerson", StringComparison.OrdinalIgnoreCase) && x.ReceiveNotifications);
            var hybridEvents = notificationSettings.EventTypes
                .Where(x => x.EventType.Equals("Hybrid", StringComparison.OrdinalIgnoreCase) && x.ReceiveNotifications);

            if (inPersonEvents.Any() && hybridEvents.Any())
            {
                sb.AppendLine($"#In-person and hybrid events ({inPersonAndHybridTotalCount} events)");
                sb.AppendLine();
            }
            else if (inPersonEvents.Any())
            {
                sb.AppendLine($"#In-person events ({inPersonAndHybridTotalCount} events)");
                sb.AppendLine();
            }
            else if (hybridEvents.Any())
            {
                sb.AppendLine($"#Hybrid events ({inPersonAndHybridTotalCount} events)");
                sb.AppendLine();
            }

            foreach (var locationEvents in inPersonAndHybridEvents)
            {
                AppendLocationEvents(sb, locationEvents, null, EventFormat.InPerson, EventFormat.Hybrid);
            }
        }

        if (onlineEvents.Any())
        {
            int totalCountSum = onlineEvents.Sum(e => e.TotalCount);

            sb.AppendLine($"#Online events ({onlineTotalCount} events)");
            sb.AppendLine();

            AppendLocationEvents(sb, onlineEventListing, totalCountSum, EventFormat.Online);
        }

        return sb.ToString();
    }

    private void AppendLocationEvents(StringBuilder sb, EventListingDTO locationEvents, int? onlineTotalCount, params EventFormat[] formatsToInclude)
    {
        var filteredEvents = locationEvents.CalendarEvents
            .Where(ev => formatsToInclude.Contains(ev.EventFormat))
            .ToList();

        if (!filteredEvents.Any())
            return;

        if (!formatsToInclude.Contains(EventFormat.Online))
        {
            var locationHeaderText = locationEvents.Radius == 0
                ? $"##{locationEvents.Location}, Across England ({locationEvents.TotalCount} events)"
                : $"##{locationEvents.Location}, within {locationEvents.Radius} miles ({locationEvents.TotalCount} events)";
            sb.AppendLine(locationHeaderText);
            sb.AppendLine();
        }

        var locationUrlText = locationEvents.Radius == 0
            ? $"across England ,{locationEvents.Location}"
            : $"within {locationEvents.Radius} miles  of {locationEvents.Location}";

        var eventsDisplayed = 0;

        foreach (var calendarEvent in filteredEvents)
        {
            var calendarEventUrl = _applicationConfiguration.ApprenticeAanBaseUrl + "/network-events/" + calendarEvent.CalendarEventId;

            sb.AppendLine($"##[{calendarEvent.Title}]({calendarEventUrl})");
            sb.AppendLine();
            sb.AppendLine(calendarEvent.Summary);
            sb.AppendLine();
            sb.AppendLine($"Date: {calendarEvent.Start.ToString("dd MMMM yyyy")}");
            sb.AppendLine();
            sb.AppendLine($"Time: {calendarEvent.Start.ToString("htt").ToUpper()} to {calendarEvent.End.ToString("htt").ToUpper()}");
            sb.AppendLine();

            if (calendarEvent.EventFormat != EventFormat.Online)
            {
                sb.AppendLine($"Where: {calendarEvent.Location}");
                sb.AppendLine();
                sb.AppendLine($"Distance: {calendarEvent.Distance} miles");
                sb.AppendLine();
            }
            sb.AppendLine($"Event type: {calendarEvent.CalendarName.ToString()}");

            eventsDisplayed++;

            if (eventsDisplayed >= MaxEventsPerLocation)
            {
                var allEventsUrl = _applicationConfiguration.ApprenticeAanBaseUrl + "/network-events";
                var allEventsUrlText = calendarEvent.EventFormat == EventFormat.Online ?
                    $"See all {onlineTotalCount} upcoming online events" :
                    $"See all {locationEvents.TotalCount} upcoming events {locationUrlText}";
                var allEventsText = calendarEvent.EventFormat == EventFormat.Online
                    ? $"We're only showing you the next {MaxEventsPerLocation} online events"
                    : $"We're only showing you the next {MaxEventsPerLocation} events in {locationEvents.Location}";

                sb.AppendLine();
                sb.AppendLine($"^ {allEventsText}. [{allEventsUrlText}]({allEventsUrl}).");
                sb.AppendLine();
                sb.AppendLine("---");
                break;
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("---");
            }
        }

        sb.AppendLine();
    }
}