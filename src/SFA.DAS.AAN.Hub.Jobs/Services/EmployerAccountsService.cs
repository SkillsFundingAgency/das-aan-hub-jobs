using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using System;
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

    public async Task<EmployerMember> GetEmployerUserAccounts(Guid userRef)
    {
        var result = await _outerApiClient.GetEmployerMember(userRef, CancellationToken.None);
        return result.GetContent();
    }
}