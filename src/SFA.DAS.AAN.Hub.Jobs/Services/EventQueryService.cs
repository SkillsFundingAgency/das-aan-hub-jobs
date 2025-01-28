using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public class EventQueryService : IEventQueryService
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<EventQueryService> _logger;

    public EventQueryService(IOuterApiClient outerApiClient, ILogger<EventQueryService> logger)
    {
        _outerApiClient = outerApiClient;
        _logger = logger;
    }

    public async Task<List<EventListingDTO>> GetEventListings(EventNotificationSettings notificationSettings, List<EventFormat> eventFormats, CancellationToken cancellationToken)
    {
        var eventListings = new List<EventListingDTO>();

        foreach (var locationSetting in notificationSettings.Locations)
        {
            foreach (var eventType in eventFormats)
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

                eventListings.Add(new EventListingDTO
                {
                    TotalCount = eventList.TotalCount,
                    CalendarEvents = eventList.CalendarEvents,
                    Location = locationSetting.Name,
                    Radius = locationSetting.Radius
                });
            }
        }

        return eventListings;
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
}
