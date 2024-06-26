﻿using Microsoft.Extensions.Logging;
using RestEase;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface ISynchroniseApprenticeDetailsService
{
    Task<(int, int)> SynchroniseApprentices(CancellationToken cancellationToken);
}

public class SynchroniseApprenticeDetailsService : ISynchroniseApprenticeDetailsService
{
    private readonly IApprenticeAccountsApiClient _apprenticeAccountsApiClient;
    private readonly IJobAuditRepository _jobAuditRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAanDataContext _aanDataContext;
    private readonly ISynchroniseApprenticeDetailsRepository _synchroniseApprenticeDetailsRepository;

    public SynchroniseApprenticeDetailsService(
        IApprenticeAccountsApiClient apprenticeAccountsApiClient,
        IJobAuditRepository jobAuditRepository,
        IMemberRepository memberRepository,
        IAanDataContext aanDataContext,
        ISynchroniseApprenticeDetailsRepository synchroniseApprenticeDetailsRepository
    )
    {
        _apprenticeAccountsApiClient = apprenticeAccountsApiClient;
        _jobAuditRepository = jobAuditRepository;
        _memberRepository = memberRepository;
        _aanDataContext = aanDataContext;
        _synchroniseApprenticeDetailsRepository = synchroniseApprenticeDetailsRepository;
    }

    public async Task<(int, int)> SynchroniseApprentices(CancellationToken cancellationToken)
    {
        JobAudit audit = new JobAudit()
        {
            JobName = nameof(SynchroniseApprenticeDetailsFunction),
            StartTime = DateTime.UtcNow
        };

        var members = await _memberRepository.GetActiveApprenticeMembers(cancellationToken);

        if (members is null || members.Count == 0)
            return await RecordAuditAndReturnDefault(audit, cancellationToken);

        var apprenticeIds = members.Select(a => a.Apprentice.ApprenticeId).ToArray();

        var response = await QueryApprenticeApi(apprenticeIds, cancellationToken);

        if(response.ResponseMessage.StatusCode != HttpStatusCode.OK)
            return default;

        var responseObject = response.GetContent();

        if (!responseObject.Apprentices.Any())
            return await RecordAuditAndReturnDefault(audit, cancellationToken);

        var processedMembers = UpdateApprenticeDetails(members, responseObject);

        await _synchroniseApprenticeDetailsRepository.AddJobAudit(audit, response.StringContent, cancellationToken);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return processedMembers;
    }

    private async Task<(int, int)> RecordAuditAndReturnDefault(JobAudit audit, CancellationToken cancellationToken)
    {
        await _synchroniseApprenticeDetailsRepository.AddJobAudit(audit, null, cancellationToken);
        await _aanDataContext.SaveChangesAsync(cancellationToken);
        return (default, default);
    }

    private async Task<Response<ApprenticeSyncResponseDto>> QueryApprenticeApi(Guid[] apprenticeIds, CancellationToken cancellationToken)
    {
        var lastJobAudit = await _jobAuditRepository.GetMostRecentJobAudit(nameof(SynchroniseApprenticeDetailsFunction), cancellationToken);

        return await _apprenticeAccountsApiClient.SynchroniseApprentices(
            apprenticeIds,
            lastJobAudit?.StartTime, 
            cancellationToken
        );
    }

    private (int, int) UpdateApprenticeDetails(List<Member> members, ApprenticeSyncResponseDto apprenticeSyncResponseDto)
    {
        var apprentices = apprenticeSyncResponseDto.Apprentices;

        int updatedMembers = 0;

        foreach (var member in members)
        { 
            var apprentice = Array.Find(apprentices, a => a.ApprenticeID == member.Apprentice.ApprenticeId);

            if (apprentice == null)
                continue;

            _synchroniseApprenticeDetailsRepository.UpdateMemberDetails(
                member, 
                apprentice.FirstName, 
                apprentice.LastName, 
                apprentice.Email
            );

            updatedMembers++;
        }

        return (members.Count, updatedMembers);
    }
}
