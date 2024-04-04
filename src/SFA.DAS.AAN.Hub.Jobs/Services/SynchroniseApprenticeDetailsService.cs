using Microsoft.Extensions.Logging;
using RestEase;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface ISynchroniseApprenticeDetailsService
{
    Task<int> SynchroniseApprentices(CancellationToken cancellationToken);
}

public class SynchroniseApprenticeDetailsService : ISynchroniseApprenticeDetailsService
{
    private readonly ILogger<SynchroniseApprenticeDetailsService> _logger;
    private readonly IApprenticeAccountsApiClient _apprenticeAccountsApiClient;
    private readonly IApprenticeRepository _apprenticeshipRespository;
    private readonly IJobAuditRepository _jobAuditRepository;
    private readonly IMemberRepository _memberRepository;

    public SynchroniseApprenticeDetailsService(
        ILogger<SynchroniseApprenticeDetailsService> logger,
        IApprenticeAccountsApiClient apprenticeAccountsApiClient,
        IApprenticeRepository apprenticeshipRespository,
        IJobAuditRepository jobAuditRepository,
        IMemberRepository memberRepository
    )
    {
        _logger = logger;
        _apprenticeAccountsApiClient = apprenticeAccountsApiClient;
        _apprenticeshipRespository = apprenticeshipRespository;
        _jobAuditRepository = jobAuditRepository;
        _memberRepository = memberRepository;
    }

    public async Task<int> SynchroniseApprentices(CancellationToken cancellationToken)
    {
        JobAudit audit = new JobAudit()
        {
            JobName = nameof(SynchroniseApprenticeDetailsFunction),
            StartTime = DateTime.UtcNow
        };

        try
        {
            var apprentices = await _apprenticeshipRespository.GetApprentices(cancellationToken);

            if (apprentices is null || apprentices.Count == 0)
            {
                await RecordAudit(audit, null, cancellationToken);
                return default;
            }

            var response = await QueryApprenticeApi(cancellationToken, apprentices);

            if(response is null)
            {
                await RecordAudit(audit, null, cancellationToken);
                return default;
            }

            var responseObject = response.GetContent();

            if (responseObject.Apprentices.Count() == 0)
            {
                await RecordAudit(audit, null, cancellationToken);
                return default;
            }

            int updatedApprenticeCount = await UpdateApprenticeDetails(responseObject, cancellationToken);

            await RecordAudit(audit, response, cancellationToken);

            return updatedApprenticeCount;
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "{MethodName} Failed.", nameof(SynchroniseApprentices));
            await RecordAudit(audit, null, cancellationToken);
            return default;
        }
    }

    private async Task<Response<ApprenticeSyncResponseDto>> QueryApprenticeApi(CancellationToken cancellationToken, List<Apprentice> apprentices)
    {
        var lastJobAudit = await _jobAuditRepository.GetMostRecentJobAudit(cancellationToken);

        var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToArray();

        return await _apprenticeAccountsApiClient.SynchroniseApprentices(
            apprenticeIds,
            lastJobAudit?.StartTime, 
            cancellationToken
        );
    }

    private async Task<int> UpdateApprenticeDetails(ApprenticeSyncResponseDto apprenticeSyncResponseDto, CancellationToken cancellationToken)
    {
        var apprentices = await _apprenticeshipRespository.GetApprentices(
            apprenticeSyncResponseDto.Apprentices.Select(a => a.ApprenticeID).ToArray(),
            cancellationToken
        );

        if (apprentices is null || apprentices.Count == 0)
            return default;

        var memberIds = apprentices.Select(a => a.MemberId).ToArray();

        var members = await _memberRepository.GetMembers(memberIds, cancellationToken);

        foreach (var member in members)
        { 
            var apprentice = apprentices.Find(a => a.MemberId == member.Id);

            if (apprentice == null)
                continue;

            var apprenticeResponse = Array.Find(apprenticeSyncResponseDto.Apprentices, a => a.ApprenticeID == apprentice.ApprenticeId);

            if (apprenticeResponse == null)
                continue;

            member.FirstName = apprenticeResponse.FirstName;
            member.LastName = apprenticeResponse.LastName;
            member.Email = apprenticeResponse.Email;
        }

        await _memberRepository.UpdateMembers(members, cancellationToken);

        return members.Count;
    }

    private async Task RecordAudit(JobAudit jobAudit, Response<ApprenticeSyncResponseDto> response, CancellationToken cancellationToken)
    {
        try
        {
            jobAudit.EndTime = DateTime.UtcNow;
            jobAudit.Notes = response?.StringContent;
            await _jobAuditRepository.RecordAudit(jobAudit, cancellationToken);
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "Unable to record an audit record for {JobName}", jobAudit.JobName);
        }
    }
}
