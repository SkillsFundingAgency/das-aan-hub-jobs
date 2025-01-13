using RestEase;
using SFA.DAS.AAN.Hub.Data.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Clients;

public interface IOuterApiClient
{
    [Get("/members/{memberId}")]
    Task<GetMemberByIdQueryResult> GetMemberById([Path] Guid memberId, CancellationToken cancellationToken);

    [Get("CalendarEvents")]
    Task<GetCalendarEventsQueryResult> GetCalendarEvents([Header("X-RequestedByMemberId")] Guid requestedByMemberId, [QueryMap] IDictionary<string, string[]> parameters, CancellationToken cancellationToken);
}