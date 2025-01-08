﻿using RestEase;
using SFA.DAS.AAN.Hub.Data.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Clients;

public interface IOuterApiClient
{
    //[Get("/employers/{userRef}")]
    //[AllowAnyStatusCode]
    //Task<Response<EmployerMember>> GetEmployerMember([Path] Guid userRef, CancellationToken cancellationToken);

    //[Get("/employeraccounts/{userId}")]
    //Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("CalendarEvents")]
    Task<GetCalendarEventsQueryResult> GetCalendarEvents([Header("X-RequestedByMemberId")] Guid requestedByMemberId, [QueryMap] IDictionary<string, string[]> parameters, CancellationToken cancellationToken);


    //[Get("/calendarevents/{calendarEventId}")]
    //[AllowAnyStatusCode]
    //Task<Response<CalendarEvent>> GetCalendarEventDetails([Path] Guid calendarEventId,
    //[Header(RequestHeaders.RequestedByMemberIdHeader)] Guid requestedByMemberId,
    //CancellationToken cancellationToken);

    //[Get("MemberNotificationSettings/{memberId}")]
    //Task<GetMemberNotificationSettingsResponse> GetMemberNotificationSettings([Path] Guid memberId, CancellationToken cancellationToken);

}

public class GetCalendarEventsQueryResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
    public bool IsInvalidLocation { get; set; }
    // public List<Region> Regions { get; set; } = [];
    // public List<Calendar> Calendars { get; set; } = [];
}