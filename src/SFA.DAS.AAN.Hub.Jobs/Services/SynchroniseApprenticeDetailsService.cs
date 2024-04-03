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

            if (apprentices is null || !apprentices.Any())
            {
                await RecordAudit(cancellationToken, audit, null);
                return default;
            }

            var response = await QueryApprenticeApi(cancellationToken, apprentices);

            int updatedApprenticeCount = await UpdateApprenticeDetails(cancellationToken, response);

            await RecordAudit(cancellationToken, audit, response);

            return updatedApprenticeCount;
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "{MethodName} Failed.", nameof(SynchroniseApprentices));
            await RecordAudit(cancellationToken, audit, null);
            return default;
        }
    }

    private async Task<Response<ApprenticeSyncResponseDto>> QueryApprenticeApi(CancellationToken cancellationToken, List<Apprentice> apprentices)
    {
        try
        {
            var lastJobAudit = await _jobAuditRepository.GetMostRecentJobAudit(cancellationToken);

            var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToArray();

            return await _apprenticeAccountsApiClient.SynchroniseApprentices(
                apprenticeIds,
                lastJobAudit?.StartTime, 
                cancellationToken
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to query apprentice API.");
            throw;
        }
    }

    private async Task<int> UpdateApprenticeDetails(CancellationToken cancellationToken, Response<ApprenticeSyncResponseDto> apprenticeSyncResponseDto)
    {
        try
        {
            var responseObject = apprenticeSyncResponseDto.GetContent();

            if(responseObject is null || !responseObject.Apprentices.Any())
                return default;

            var apprentices = await _apprenticeshipRespository.GetApprentices(cancellationToken, responseObject.Apprentices.Select(a => a.ApprenticeID).ToArray());

            if(!apprentices.Any())
                return default;

            var members = await _memberRepository.GetMembers(cancellationToken, apprentices.Select(a => a.MemberId).ToArray());

            foreach (var member in members)
            { 
                var apprentice = apprentices.FirstOrDefault(a => a.MemberId == member.Id);

                if (apprentice == null)
                    continue;

                var apprenticeResponse = responseObject.Apprentices.FirstOrDefault(a => a.ApprenticeID == apprentice.ApprenticeId);

                if (apprenticeResponse != null)
                {
                    member.FirstName = apprenticeResponse.FirstName;
                    member.LastName = apprenticeResponse.LastName;
                    member.Email = apprenticeResponse.Email;
                }
                else
                {
                    _logger.LogWarning("Apprentice with ID {ApprenticeId} not found in the database.", apprenticeResponse.ApprenticeID.ToString());
                }
            }

            await _memberRepository.UpdateMembers(cancellationToken, members);

            return members.Count;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update apprentice details.");
            throw;
        }
    }

    private async Task RecordAudit(CancellationToken cancellationToken, JobAudit jobAudit, Response<ApprenticeSyncResponseDto>? response)
    {
        try
        {
            jobAudit.EndTime = DateTime.UtcNow;
            jobAudit.Notes = response?.StringContent;
            await _jobAuditRepository.RecordAudit(cancellationToken, jobAudit);
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "Unable to record an audit record for {JobName}", jobAudit.JobName);
        }
    }
}
