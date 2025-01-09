using RestEase;
using SFA.DAS.AAN.Hub.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Clients;

public interface IOuterApiClient
{
    //[Get("/employeraccounts/{userId}")]
    //Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    //[Get("/employers/{userRef}")]
    //[AllowAnyStatusCode]
    //Task<Response<EmployerMember>> GetEmployerMember([Path] Guid userRef, CancellationToken cancellationToken);



    [Get("/members/{memberId}")]
    Task<GetMemberByIdQueryResult> GetMemberById([Path] Guid memberId, CancellationToken cancellationToken);

    [Get("CalendarEvents")]
    Task<GetCalendarEventsQueryResult> GetCalendarEvents([Header("X-RequestedByMemberId")] Guid requestedByMemberId, [QueryMap] IDictionary<string, string[]> parameters, CancellationToken cancellationToken);
}

public class GetCalendarEventsQueryResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
    public bool IsInvalidLocation { get; set; }
}

public class GetMemberByIdQueryResult
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? OrganisationName { get; set; }
    public int? RegionId { get; set; }
    public string UserType { get; set; } = null!;
    public bool? IsRegionalChair { get; set; }
    public string FullName { get; set; } = null!;
    public long EmployerAccountId { get; set; }
    public Guid UserRef { get; set; }
}