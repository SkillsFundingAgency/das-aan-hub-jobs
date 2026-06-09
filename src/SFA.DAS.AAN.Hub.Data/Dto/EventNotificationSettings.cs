namespace SFA.DAS.AAN.Hub.Data.Dto;

public class EventNotificationSettings
{
    public MemberDetails MemberDetails { get; set; } = new MemberDetails();
    public bool? ReceiveNotifications { get; set; }
    public List<NotificationEventType> EventTypes = [];
    public List<Location> Locations { get; set; } = [];

    public class NotificationEventType
    {
        public string EventType { get; set; } = string.Empty;
        public bool ReceiveNotifications { get; set; }
    }

    public class Location
    {
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
