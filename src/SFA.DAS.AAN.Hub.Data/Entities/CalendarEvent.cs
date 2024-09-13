namespace SFA.DAS.AAN.Hub.Data.Entities;
public class CalendarEvent
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string EventFormat { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int CalendarId { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public virtual Member Member { get; set; } =  null!;
    public virtual Calendar Calender { get; set; } = null!;
}
