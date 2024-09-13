using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IEventSignUpNotificationService
{
    Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken);
}

public class EventSignUpNotificationService : IEventSignUpNotificationService
{
    public const string LinkTokenKey = "link";

    private readonly IEventSignUpNotificationRepository _eventSignUpNotificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public EventSignUpNotificationService(
        IEventSignUpNotificationRepository eventSignUpNotificationRepository,
        ILogger<NotificationService> logger)
    {
        _eventSignUpNotificationRepository = eventSignUpNotificationRepository;
        _logger = logger;
    }

    public async Task<int> ProcessEventSignUpNotification(CancellationToken cancellationToken)
    {
        var pendingEventSignUpNotifications = await _eventSignUpNotificationRepository.GetEventSignUpNotification();

        if (pendingEventSignUpNotifications.Count == 0) return 0;

        //Handle SendNotification;

        return pendingEventSignUpNotifications.Count;
    }
}
