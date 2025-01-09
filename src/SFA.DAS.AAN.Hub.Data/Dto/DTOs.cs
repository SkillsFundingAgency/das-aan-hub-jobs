using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Data.Dto;


public record GetEmployerUserAccountsResponse(string FirstName, string LastName, string EmployerUserId, bool IsSuspended, IEnumerable<EmployerUserAccountItem> UserAccountResponse);

public record EmployerUserAccountItem(string EncodedAccountId, string DasAccountName, string Role);

public class EmployerMember
{
    public Guid MemberId { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}


public class UserAccountsApiResponse
{
    public List<UserAccountsApiResponseItem> UserAccounts { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string EmployerUserId { get; set; }
    public bool IsSuspended { get; set; }


    public static implicit operator UserAccountsApiResponse(GetAccountsQueryResult source)
    {
        var userAccounts = source?.UserAccountResponse == null
            ? new List<UserAccountsApiResponseItem>()
            : source.UserAccountResponse.Select(c => (UserAccountsApiResponseItem)c).ToList();

        return new UserAccountsApiResponse
        {
            EmployerUserId = source?.EmployerUserId,
            FirstName = source?.FirstName,
            LastName = source?.LastName,
            IsSuspended = source?.IsSuspended ?? false,
            UserAccounts = userAccounts
        };
    }
}

public class UserAccountsApiResponseItem
{
    public string EncodedAccountId { get; set; }
    public string DasAccountName { get; set; }
    public string Role { get; set; }
    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }

    public static implicit operator UserAccountsApiResponseItem(AccountUser source)
    {
        return new UserAccountsApiResponseItem
        {
            DasAccountName = source.DasAccountName,
            EncodedAccountId = source.EncodedAccountId,
            Role = source.Role,
            ApprenticeshipEmployerType = source.ApprenticeshipEmployerType
        };
    }
}

public class GetAccountsQueryResult
{
    public IEnumerable<AccountUser> UserAccountResponse { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmployerUserId { get; set; }
    public bool IsSuspended { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApprenticeshipEmployerType : byte
{
    NonLevy = 0,
    Levy = 1,
    Unknown = 2,
}

public class AccountUser
{
    public string DasAccountName { get; set; }
    public string EncodedAccountId { get; set; }
    public string Role { get; set; }
    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
}