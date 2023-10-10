using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface INotificationService
{
    Task<int> ProcessNotificationBatch();
}

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationRepository;
    private readonly IOptions<ApplicationConfiguration> _applicationConfigurationOptions;

    public NotificationService(INotificationsRepository notificationRepository, IOptions<ApplicationConfiguration> applicationConfigurationOptions)
    {
        _notificationRepository = notificationRepository;
        _applicationConfigurationOptions = applicationConfigurationOptions;
    }

    public async Task<int> ProcessNotificationBatch()
    {
        var applicationConfiguration = _applicationConfigurationOptions.Value;

        var pendingNotification = await _notificationRepository.GetPendingNotifications(applicationConfiguration.NotificationBatchSize);

        return pendingNotification.Count;
    }
}
