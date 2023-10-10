using System;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration;

public class ApplicationConfiguration
{
    public Uri ApprenticeAanRouteUrl { get; set; }
    public Uri EmployerAanRouteUrl { get; set; }
    public NotificationsConfiguration Notifications { get; set; }
}
