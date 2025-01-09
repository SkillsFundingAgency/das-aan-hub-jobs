﻿using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.Encoding;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public class EmployerAccountsService : IEmployerAccountsService
{
    private readonly IOuterApiClient _outerApiClient;
    //private readonly IEncodingService _encodingService;
    private readonly ILogger<EmployerAccountsService> _logger;

    public EmployerAccountsService(IOuterApiClient outerApiClient, 
        //IEncodingService encodingService,
        ILogger<EmployerAccountsService> logger)
    {
        _outerApiClient = outerApiClient;
        //_encodingService = encodingService;
        _logger = logger;
    }

    public async Task<string> GetEmployerUserAccounts(Guid memberId)
    {
        var result = await _outerApiClient.GetMemberById(memberId, CancellationToken.None);

        _logger.LogInformation($"employer account id: {result.EmployerAccountId}");
        //var employerHashedAccountId = _encodingService.Encode(result.GetContent().Name, EncodingType.AccountId);

        return result.EmployerAccountId.ToString();
    }
}