using System;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration;

public class ApplicationConfiguration
{
    public Uri ApprenticeAanBaseUrl { get; set; }
    public Uri EmployerAanBaseUrl { get; set; }
    public NotificationsConfiguration Notifications { get; set; }
}
