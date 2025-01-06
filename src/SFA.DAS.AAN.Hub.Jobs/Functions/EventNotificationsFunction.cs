using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.Functions
{
    public class EventNotificationsFunction
    {
        private readonly IEventSignUpNotificationService _notificationService;
        private readonly ILogger<EventSignUpNotificationFunction> _logger;
        public EventNotificationsFunction(IEventSignUpNotificationService notificationService, ILogger<EventSignUpNotificationFunction> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Function(nameof(EventNotificationsFunction))]
        public async Task Run([TimerTrigger("%EmployerEventNotificationsFunction%", RunOnStartup = false)] TimerInfo timer, CancellationToken cancellationToken)
        {
            _logger.LogInformation("EmployerEventNotificationsFunction has been triggered.");

            var count = await _notificationService.ProcessEventSignUpNotification(cancellationToken);

            _logger.LogInformation("Processed {count} Event SignUp Notifications", count);
        }
    }
}
