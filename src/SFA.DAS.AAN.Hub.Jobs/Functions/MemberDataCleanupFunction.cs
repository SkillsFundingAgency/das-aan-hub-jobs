﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;
public class MemberDataCleanupFunction
{
    private readonly IMemberDataCleanupService _memberDataCleanupService;
    private readonly ILogger<MemberDataCleanupFunction> _logger;
    public MemberDataCleanupFunction(IMemberDataCleanupService memberDataCleanupService, ILogger<MemberDataCleanupFunction> logger)
    {
        _memberDataCleanupService = memberDataCleanupService;
        _logger = logger;
    }

    [Function(nameof(MemberDataCleanupFunction))]
    public async Task Run([TimerTrigger("%MemberDataCleanupFunctionSchedule%", RunOnStartup = false)] TimerInfo timer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("MemberDataCleanupFunction has been triggered.");

        var count = await _memberDataCleanupService.ProcessMemberDataCleanup(cancellationToken);

        _logger.LogInformation("Data anonymised for {count} members.", count);
    }
}
