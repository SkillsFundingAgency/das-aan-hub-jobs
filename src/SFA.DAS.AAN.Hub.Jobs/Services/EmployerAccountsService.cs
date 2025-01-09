using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public class EmployerAccountsService : IEmployerAccountsService
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountsService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<UserAccountsApiResponse> GetEmployerUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.GetAccountUsers(userId, email, CancellationToken.None);
        return result;
    }

    //private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response) =>
    //    new(response.IsSuspended, response.FirstName, response.LastName, response.EmployerUserId, response.UserAccountResponse.Select(u => new EmployerIdentifier(u.EncodedAccountId, u.DasAccountName, u.Role)).ToList());

    //    private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response) =>
    //        new(response.IsSuspended, response.FirstName, response.LastName, response.EmployerUserId, response.UserAccountResponse.Select(u => new EmployerIdentifier(u.EncodedAccountId, u.DasAccountName, u.Role)).ToList());
    //}
}

