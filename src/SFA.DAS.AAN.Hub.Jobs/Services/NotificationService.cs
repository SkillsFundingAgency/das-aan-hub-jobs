using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Models;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface INotificationService
{
    Task<int> ProcessNotificationBatch();
}

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationRepository;
    private readonly IOptions<ApplicationConfiguration> _applicationConfigurationOptions;
    private readonly IMessagingService _messagingService;

    public NotificationService(INotificationsRepository notificationRepository, IOptions<ApplicationConfiguration> applicationConfigurationOptions, IMessagingService messagingService)
    {
        _notificationRepository = notificationRepository;
        _applicationConfigurationOptions = applicationConfigurationOptions;
        _messagingService = messagingService;
    }

    public async Task<int> ProcessNotificationBatch()
    {
        var applicationConfiguration = _applicationConfigurationOptions.Value;

        var pendingNotification = await _notificationRepository.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize);

        var commands = pendingNotification.Select(n => new SendEmailCommand(n, applicationConfiguration)).ToArray().Select(c => _messagingService.SendMessage(c));

        await Task.WhenAll(commands);

        return pendingNotification.Count;
    }
}
