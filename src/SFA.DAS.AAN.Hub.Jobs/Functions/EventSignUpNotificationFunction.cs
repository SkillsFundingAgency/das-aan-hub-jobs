using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;

public class EventSignUpNotificationFunction
{
    private readonly IEventSignUpNotificationService _notificationService;
    private readonly ILogger<EventSignUpNotificationFunction> _logger;
    public EventSignUpNotificationFunction(IEventSignUpNotificationService notificationService, ILogger<EventSignUpNotificationFunction> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [Function(nameof(EventSignUpNotificationFunction))]
    public async Task Run([TimerTrigger("%EventSignUpNotificationFunctionSchedule%", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventSignUpNotificationFunction has been triggered.");

        var count = await _notificationService.ProcessEventSignUpNotification(cancellationToken);

        _logger.LogInformation("Processed {count} Event SignUp Notifications", count);
    }
}