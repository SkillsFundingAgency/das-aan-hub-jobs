using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;

public class SendNotificationsFunction
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendNotificationsFunction> _logger;
    public SendNotificationsFunction(INotificationService notificationService, ILogger<SendNotificationsFunction> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [Function(nameof(SendNotificationsFunction))]
    public async Task Run([TimerTrigger("%SendNotificationsFunctionSchedule%", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SendNotificationsFunction has been triggered.");

        var count = await _notificationService.ProcessNotificationBatch(cancellationToken);

        _logger.LogInformation("Processed {count} notifications.", count);
    }
}