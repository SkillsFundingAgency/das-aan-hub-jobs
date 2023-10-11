using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Models;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface INotificationService
{
    Task<int> ProcessNotificationBatch(CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationRepository;
    private readonly IOptions<ApplicationConfiguration> _applicationConfigurationOptions;
    private readonly IMessagingService _messagingService;
    private readonly IAanDataContext _aanDataContext;

    public NotificationService(
        INotificationsRepository notificationRepository,
        IOptions<ApplicationConfiguration> applicationConfigurationOptions,
        IMessagingService messagingService,
        IAanDataContext aanDataContext)
    {
        _notificationRepository = notificationRepository;
        _applicationConfigurationOptions = applicationConfigurationOptions;
        _messagingService = messagingService;
        _aanDataContext = aanDataContext;
    }

    public async Task<int> ProcessNotificationBatch(CancellationToken cancellationToken)
    {
        var applicationConfiguration = _applicationConfigurationOptions.Value;

        var pendingNotifications = await _notificationRepository.GetPendingNotifications(applicationConfiguration.Notifications.BatchSize);

        var commands = pendingNotifications.Select(n => new SendEmailCommand(n, applicationConfiguration)).Select(c => _messagingService.SendMessage(c));

        var now = DateTime.UtcNow;
        pendingNotifications.ForEach(n => n.SentTime = now);

        await Task.WhenAll(commands);

        await _aanDataContext.SaveChangesAsync(cancellationToken);

        return pendingNotifications.Count;
    }
}
