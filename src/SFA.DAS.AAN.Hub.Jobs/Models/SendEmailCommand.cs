using System.Collections.Generic;
using System.Text.Json;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Jobs.Models;

public class SendEmailCommand
{
    public string TemplateId { get; }
    public string RecipientsAddress { get; }
    public IReadOnlyDictionary<string, string> Tokens { get; }

    public SendEmailCommand(Notification notification)
    {
        TemplateId = notification.TemplateName;
        RecipientsAddress = notification.Member.Email;
        Tokens = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);
    }
}
