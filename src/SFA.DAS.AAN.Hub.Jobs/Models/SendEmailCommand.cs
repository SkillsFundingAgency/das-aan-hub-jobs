using System;
using System.Collections.Generic;
using System.Text.Json;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Jobs.Configuration;

namespace SFA.DAS.AAN.Hub.Jobs.Models;

public class SendEmailCommand
{
    public const string UserTypeApprentice = "Apprentice";
    public const string LinkTokenKey = "Link";
    public string TemplateId { get; }
    public string RecipientsAddress { get; }
    public IDictionary<string, string> Tokens { get; }

    public SendEmailCommand(Notification notification, ApplicationConfiguration applicationConfiguration)
    {
        TemplateId = applicationConfiguration.Notifications.Templates[notification.TemplateName];
        RecipientsAddress = notification.Member.Email;
        Tokens = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Tokens);
        var link = new Uri(notification.Member.UserType == UserTypeApprentice ? applicationConfiguration.ApprenticeAanRouteUrl : applicationConfiguration.EmployerAanRouteUrl, $"links/{notification.Id}");
        Tokens.Add(LinkTokenKey, link.ToString());
    }
}
