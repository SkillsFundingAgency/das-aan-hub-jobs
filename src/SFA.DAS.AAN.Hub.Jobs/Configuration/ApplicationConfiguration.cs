using System;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration;

public class ApplicationConfiguration
{
    public Uri ApprenticeAanBaseUrl { get; set; }
    public Uri EmployerAanBaseUrl { get; set; }
    public Uri AdminAanBaseUrl { get; set; }
    public ApprenticeAccountsApiConfiguration ApprenticeAccountsApiConfiguration { get; set; }
    public AanOuterApiConfiguration AanOuterApiConfiguration { get; set; }
    public ApprenticeAanOuterApiConfiguration ApprenticeAanOuterApiConfiguration { get; set; }
    public NotificationsConfiguration Notifications { get; set; }
    public MemberDataCleanupConfiguration MemberDataCleanup { get; set; }
}
