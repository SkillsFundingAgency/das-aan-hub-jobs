using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;
public class MemberDataCleanupFunction
{
    private readonly IMemberDataCleanupService _memberDataCleanupService;
    private readonly ILogger _logger;
    public MemberDataCleanupFunction(IMemberDataCleanupService memberDataCleanupService, ILogger logger)
    {
        _memberDataCleanupService = memberDataCleanupService;
        _logger = logger;
    }

    [Function(nameof(MemberDataCleanupFunction))]
    // public async Task Run([TimerTrigger("%ApplicationConfiguration:MemberDataCleanup:Schedule%", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
    public async Task Run([TimerTrigger("0 0 4 * * 1-5", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(MemberDataCleanupFunction)} has been triggered.");

        var count = await _memberDataCleanupService.ProcessMemberDataCleanup(cancellationToken);

        _logger.LogInformation($"Data anonymised for {count} members.");
    }
}
