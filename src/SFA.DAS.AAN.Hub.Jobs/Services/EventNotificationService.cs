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
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;

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
    private readonly IOuterApiClient _outerApiClient;

    public EventNotificationService(
       IEventNotificationSettingsRepository memberRepository,
       IMessageSession messageSession,
       IOptions<ApplicationConfiguration> applicationConfigurationOptions,
       ILogger<EventNotificationService> logger,
       IOuterApiClient outerApiClient)
    {
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
        _outerApiClient = outerApiClient;
    }

    public async Task<int> ProcessEventNotifications(CancellationToken cancellationToken)
    {
        var notificationSettings = await _memberRepository.GetEventNotificationSettingsAsync(cancellationToken);

        _logger.LogInformation("Number of members receiving event notifications: {count}.", notificationSettings.Count);

        if (notificationSettings.Count == 0) return 0;

        //var notificationPerEmployer = notificationSettings.GroupBy(s => s.MemberDetails.Id);
        //var firstNot = notificationSettings.First();
        //var eventListRequest = new GetNetworkEventsRequest 
        //{
        //    Location = firstNot.Locations.First().Name,
        //    EventFormat = new List<EventFormat> { EventFormat.Online },
        //    Radius = firstNot.Locations[0].Radius,
        //};

        //var eventsQuery = BuildQueryStringParameters(eventListRequest);

        //var eventList = await _outerApiClient.GetCalendarEvents(firstNot.MemberDetails.Id, eventsQuery, cancellationToken);

        //_logger.LogInformation("Number of events found: {count} for location {location}.", eventList.TotalCount, firstNot.Locations.First().Name);

        var tasks = notificationSettings.Select(n => SendEventNotificationEmails(n, cancellationToken));

        await Task.WhenAll(tasks);

        return notificationSettings.Count;
    }

    private async Task SendEventNotificationEmails(EventNotificationSettings notificationSettings, CancellationToken cancellationToken)
    {
        try
        {
            // GETTING EVENTS
            //var eventFormats = notificationSettings.EventTypes.Select(x => (Enum.TryParse(x.EventType, out EventFormat format)));
            var eventFormats = notificationSettings.EventTypes.Select(x =>
            {
                if (Enum.TryParse(x.EventType, ignoreCase: true, out EventFormat format))
                {
                    return (EventFormat?)format; // Return the parsed enum if successful
                }
                return null; // Return null if parsing failed
            })
            .Where(format => format.HasValue) // Filter out nulls (failed parses)
            .Cast<EventFormat>() // Convert the result to a list of enums
            .ToList();

            var eventListingForAllLocations = new List<EventListingDTO>(); // need a new type to have location name radius and the events

            // for each location, get a list of events..
            foreach (var locationSetting in notificationSettings.Locations)
            {
                var request = new GetNetworkEventsRequest
                {
                    Location = locationSetting.Name,
                    EventFormat = eventFormats,
                    Radius = locationSetting.Radius,
                };

                var eventsQuery = BuildQueryStringParameters(request);

                var eventList = await _outerApiClient.GetCalendarEvents(notificationSettings.MemberDetails.Id, eventsQuery, cancellationToken);

                _logger.LogInformation("Number of events found: {count} for location {location}.", eventList.TotalCount, locationSetting.Name);

                eventListingForAllLocations.Add(new EventListingDTO 
                {
                    TotalCount = eventList.TotalCount,
                    CalendarEvents = eventList.CalendarEvents,
                    Location = locationSetting.Name,
                    Radius = locationSetting.Radius
                });
            }

            // SENDER
            var command = CreateSendCommand(notificationSettings, eventListingForAllLocations, cancellationToken);
            _logger.LogInformation("Sending email to member {memberId}.", notificationSettings.MemberDetails.Id);
            await _messageSession.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED to {memberId}!", notificationSettings.MemberDetails.Id);
        }
    }

    private SendEmailCommand CreateSendCommand(EventNotificationSettings notificationSetting, List<EventListingDTO> events, CancellationToken cancellationToken)
    {
        var targetEmail = notificationSetting.MemberDetails.Email;
        var firstName = notificationSetting.MemberDetails.FirstName;
        var unsubscribeURL = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "accounts/" + "TODO/" + "event-notification-settings"; // TODO

        var tokens = new Dictionary<string, string>
            {
                { "first_name", firstName },
                { "event_count", "1" }, // TODO
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

    public static Dictionary<string, string[]> BuildQueryStringParameters(GetNetworkEventsRequest request)
    {
        Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            dictionary.Add("keyword", new string[1] { request.Keyword.Trim() });
        }

        string text = "";
        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            dictionary.Add("location", new string[1] { request.Location });
            dictionary.Add("radius", new string[1] { request.Radius.ToString() ?? string.Empty });
            text = (string.IsNullOrWhiteSpace(request.OrderBy) ? "soonest" : request.OrderBy);
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            dictionary.Add("orderBy", new string[1] { text });
        }

        if (request.FromDate.HasValue)
        {
            dictionary.Add("fromDate", new string[1] { request.FromDate.Value.ToString("yyyy-MM-dd") });
        }

        if (request.ToDate.HasValue)
        {
            dictionary.Add("toDate", new string[1] { request.ToDate.Value.ToString("yyyy-MM-dd") });
        }

        dictionary.Add("eventFormat", request.EventFormat.Select((EventFormat format) => format.ToString()).ToArray());
        dictionary.Add("calendarId", request.CalendarId.Select((int cal) => cal.ToString()).ToArray());
        dictionary.Add("regionId", request.RegionId.Select((int region) => region.ToString()).ToArray());
        if (request.Page.HasValue)
        {
            dictionary.Add("page", new string[1] { request.Page?.ToString() });
        }

        if (request.PageSize.HasValue)
        {
            dictionary.Add("pageSize", new string[1] { request.PageSize?.ToString() });
        }

        return dictionary;
    }

    public class GetNetworkEventsRequest
    {
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<EventFormat> EventFormat { get; set; } = new List<EventFormat>();
        public List<int> CalendarId { get; set; } = new List<int>();
        public List<int> RegionId { get; set; } = new List<int>();
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Location { get; set; } = "";
        public int? Radius { get; set; }
        public string OrderBy { get; set; } = "";
    }

    public class EventListingDTO
    {
        public int TotalCount { get; set; }
        public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
        public string? Location { get; set; } = "";
        public int? Radius { get; set; }
    }
}
