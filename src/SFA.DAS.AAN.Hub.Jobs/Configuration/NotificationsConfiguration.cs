using System.Collections.Generic;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration;

public class NotificationsConfiguration
{
    public string Schedule { get; set; }
    public int BatchSize { get; set; }
    public int RetentionDays { get; set; }
    public Dictionary<string, string> Templates { get; set; }
}
