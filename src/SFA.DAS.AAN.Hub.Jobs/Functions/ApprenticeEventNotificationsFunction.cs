using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.Functions
{
    public class ApprenticeEventNotificationsFunction
    {
        private readonly IApprenticeEventNotificationService _notificationService;
        private readonly ILogger<ApprenticeEventNotificationsFunction> _logger;
        public ApprenticeEventNotificationsFunction(IApprenticeEventNotificationService notificationService, ILogger<ApprenticeEventNotificationsFunction> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Function(nameof(ApprenticeEventNotificationsFunction))]
        public async Task Run([TimerTrigger("%ApprenticeEventNotificationsFunctionSchedule%", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ApprenticeEventNotificationsFunctionSchedule has been triggered.");

            var count = await _notificationService.ProcessEventNotifications(cancellationToken);

            _logger.LogInformation("Processed {count} Event Notifications", count);
        }
    }
}
