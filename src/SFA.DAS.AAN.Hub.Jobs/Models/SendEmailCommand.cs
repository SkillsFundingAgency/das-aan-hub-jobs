using System.Collections.Generic;
using System.Text.Json;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Jobs.Configuration;

namespace SFA.DAS.AAN.Hub.Jobs.Models;

public class SendEmailCommand
{
    public string TemplateId { get; }
    public string RecipientsAddress { get; }
    public IReadOnlyDictionary<string, string> Tokens { get; }

    public SendEmailCommand(Notification notification, ApplicationConfiguration applicationConfiguration)
    {
        TemplateId = applicationConfiguration.Notifications.Templates[notification.TemplateName];
        RecipientsAddress = notification.Member.Email;
        Tokens = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);
    }
}
