using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Interfaces;
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
    private readonly IApprenticeAccountsApi _apprenticeAccountsApi;
    private readonly IApprenticeRepository _apprenticeshipRespository;
    private readonly IJobAuditRepository _jobAuditRepository;
    private readonly IMemberRepository _memberRepository;

    public SynchroniseApprenticeDetailsService(
        ILogger<SynchroniseApprenticeDetailsService> logger,
        IApprenticeAccountsApi apprenticeAccountsApi,
        IApprenticeRepository apprenticeshipRespository,
        IJobAuditRepository jobAuditRepository,
        IMemberRepository memberRepository
    )
    {
        _logger = logger;
        _apprenticeAccountsApi = apprenticeAccountsApi;
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
                await RecordAudit(cancellationToken, audit);
                return default;
            }

            await QueryApprenticeApi(cancellationToken, apprentices);

            int updatedApprenticeCount = await UpdateApprenticeDetails(cancellationToken);

            await RecordAudit(cancellationToken, audit);

            return updatedApprenticeCount;
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "{MethodName} Failed.", nameof(SynchroniseApprentices));
            await RecordAudit(cancellationToken, audit);
            return default;
        }
    }

    private async Task QueryApprenticeApi(CancellationToken cancellationToken, List<Apprentice> apprentices)
    {
        try
        {
            var lastJobAudit = await _jobAuditRepository.GetMostRecentJobAudit(cancellationToken);

            string apiUrl = "apprentices/sync";

            if (lastJobAudit != null)
                apiUrl += $"?updatedSinceDate={lastJobAudit.StartTime:yyyy-MM-dd}";

            var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToList();

            await _apprenticeAccountsApi.PostValueAsync(cancellationToken, apiUrl, apprenticeIds);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to query apprentice API.");
            throw;
        }
    }

    private async Task<int> UpdateApprenticeDetails(CancellationToken cancellationToken)
    {
        try
        {
            var responseObject = _apprenticeAccountsApi.GetDeserializedResponseObject<ApprenticeSyncResponseDto>();

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

    private async Task RecordAudit(CancellationToken cancellationToken, JobAudit jobAudit)
    {
        try
        {
            jobAudit.EndTime = DateTime.UtcNow;
            jobAudit.Notes = _apprenticeAccountsApi.ResponseContent;
            await _jobAuditRepository.RecordAudit(cancellationToken, jobAudit);
        }
        catch(Exception _exception)
        {
            _logger.LogError(_exception, "Unable to record an audit record for {JobName}", jobAudit.JobName);
        }
    }
}
