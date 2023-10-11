using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;

public class SendNotificationsFunction
{
    private readonly INotificationService _notificationService;

    public SendNotificationsFunction(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [FunctionName(nameof(SendNotificationsFunction))]
    public async Task Run([TimerTrigger("%ApplicationConfiguration:Notifications:Schedule%", RunOnStartup = true)] TimerInfo timer, ILogger log, CancellationToken cancellationToken)
    {
        log.LogInformation($"{nameof(SendNotificationsFunction)} has been triggered.");

        var count = await _notificationService.ProcessNotificationBatch(cancellationToken);

        log.LogInformation($"Processed {count} notifications.");
    }
}
