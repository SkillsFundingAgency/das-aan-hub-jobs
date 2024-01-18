using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;
public class MemberDataCleanupFunction
{
    private readonly IMemberDataCleanupService _memberDataCleanupService;

    public MemberDataCleanupFunction(IMemberDataCleanupService memberDataCleanupService)
    {
        _memberDataCleanupService = memberDataCleanupService;
    }

    [FunctionName(nameof(MemberDataCleanupFunction))]
    public async Task Run([TimerTrigger("%ApplicationConfiguration:MemberDataCleanup:Schedule%", RunOnStartup = true)] TimerInfo timer, ILogger log, CancellationToken cancellationToken)
    {
        log.LogInformation($"{nameof(MemberDataCleanupFunction)} has been triggered.");

        var count = await _memberDataCleanupService.ProcessMemberDataCleanup(cancellationToken);

        log.LogInformation($"Data anonymised for {count} members.");
    }
}
