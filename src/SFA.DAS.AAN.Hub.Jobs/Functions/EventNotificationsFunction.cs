using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.Functions
{
    public class EventNotificationsFunction
    {
        private readonly IEventNotificationService _notificationService;
        private readonly ILogger<EventNotificationsFunction> _logger;
        public EventNotificationsFunction(IEventNotificationService notificationService, ILogger<EventNotificationsFunction> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Function(nameof(EventNotificationsFunction))]
        public async Task Run([TimerTrigger("%EmployerEventNotificationsFunction%", RunOnStartup = false)] TimerInfo timer, CancellationToken cancellationToken)
        {
            _logger.LogInformation("EmployerEventNotificationsFunction has been triggered.");

            var count = await _notificationService.ProcessEventNotifications(cancellationToken);

            _logger.LogInformation("Processed {count} Event SignUp Notifications", count);
        }
    }
}
