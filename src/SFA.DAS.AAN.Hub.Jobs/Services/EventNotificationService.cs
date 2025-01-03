using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IEventNotificationService
{
    Task<int> ProcessEventNotifications(CancellationToken cancellationToken);
}

public class EventNotificationService : IEventNotificationService
{
    private readonly IEventNotificationSettingsRepository _memberRepository;
    private readonly ILogger<EventNotificationService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IMessageSession _messageSession;

    public EventNotificationService(
       IEventNotificationSettingsRepository memberRepository,
       IMessageSession messageSession,
       IOptions<ApplicationConfiguration> applicationConfigurationOptions,
       ILogger<EventNotificationService> logger)
    {
        _memberRepository = memberRepository;
        _messageSession = messageSession;
        _applicationConfiguration = applicationConfigurationOptions.Value;
        _logger = logger;
    }

    public async Task<int> ProcessEventNotifications(CancellationToken cancellationToken)
    {
        var notificationSettings = await _memberRepository.GetEventNotificationSettingsAsync(cancellationToken);

        _logger.LogInformation("Number of members receiving event notifications: {count}.", notificationSettings.Count);

        if (notificationSettings.Count == 0) return 0;

        var notificationPerEmployer = notificationSettings.GroupBy(s => s.MemberDetails.Id);

        var tasks = notificationPerEmployer.Select(group => SendEventNotificationEmails(group.Key, group, cancellationToken));

        await Task.WhenAll(tasks);

        return notificationSettings.Count;
    }

    private async Task SendEventNotificationEmails(Guid memberId, IEnumerable<EventNotificationSettings> notificationSettings, CancellationToken cancellationToken)
    {
        try
        {
            var command = CreateSendCommand(notificationSettings, cancellationToken);
            _logger.LogInformation("Sending email to member {memberId}.", memberId);
            await _messageSession.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending email FAILED to {memberId}!", memberId);
        }
    }

    private SendEmailCommand CreateSendCommand(IEnumerable<EventNotificationSettings> notificationSettings, CancellationToken cancellationToken)
    {
        var targetEmail = notificationSettings.First().MemberDetails.Email;
        var firstName = notificationSettings.First().MemberDetails.FirstName;
        var unsubscribeURL = _applicationConfiguration.EmployerAanBaseUrl.ToString() + "notification-settings"; // TODO

        var tokens = new Dictionary<string, string>
            {
                { "first_name", firstName },
                { "event_count", "1" }, // TODO
                { "event_listing_snippet", GetEventListingSnippet(notificationSettings) },
                { "event_formats_snippet", "TODO" },
                { "locations_snippet", "TODO" },
                { "unsubscribe_url", unsubscribeURL}
            };

        var templateId = _applicationConfiguration.Notifications.Templates["AANEmployerEventNotifications"];

        return new SendEmailCommand(templateId, targetEmail, tokens);
    }


    private string GetEventListingSnippet(IEnumerable<EventNotificationSettings> notificationSettings)
    {
        var sb = new StringBuilder();

        // TODO

        return sb.ToString();
    }
}
