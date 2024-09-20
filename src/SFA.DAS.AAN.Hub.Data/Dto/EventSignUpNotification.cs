namespace SFA.DAS.AAN.Hub.Data.Dto;
public class EventSignUpNotification
{
    public Guid CalendarEventId { get; set; }
    public Guid AdminMemberId { get; set; }
    public string CalendarName { get; set; } = null!;
    public string EventFormat { get; set; } = null!;
    public string EventTitle { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NewAmbassadorsCount { get; set; }
    public int TotalAmbassadorsCount { get; set; }
}
