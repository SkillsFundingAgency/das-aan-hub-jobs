using System;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration;

public class ApplicationConfiguration
{
    public Uri ApprenticeAanBaseUrl { get; set; }
    public Uri EmployerAanBaseUrl { get; set; }
    public ApprenticeAccountsApiConfiguration ApprenticeAccountsApiConfiguration { get; set; }
    public NotificationsConfiguration Notifications { get; set; }
    public MemberDataCleanupConfiguration MemberDataCleanup { get; set; }
}
